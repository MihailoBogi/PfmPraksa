using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalFinance.Infrastructure.Services
{
    public class TransactionImporter : ITransactionImporter
    {
        private readonly ApplicationDbContext _db;

        public TransactionImporter(ApplicationDbContext db)
            => _db = db;

        public async Task ImportAsync(IFormFile csvFile)
        {
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                PrepareHeaderForMatch = args => args.Header.Trim().Replace("-", ""),
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);
            csv.Read();
            csv.ReadHeader();

            var validEntities = new List<Transaction>();
            var errors = new List<ValidationError>();
            var row = 1;

            while (csv.Read())
            {
                row++;

                var idRaw = csv.GetField("id");
                if (!int.TryParse(idRaw, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "id",
                        Error = ValidationErrorCode.InvalidFormat,
                        Message = $"Red {row}: 'id' ({idRaw}) nije validan celobrojni tip"
                    });
                    continue;
                }

                var dateRaw = csv.GetField("date");
                if (!DateTime.TryParseExact(
                        //dateRaw,
                        //new[] { "M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                        //CultureInfo.InvariantCulture,
                        //DateTimeStyles.None,
                        //out var date))
                        dateRaw,
                        new[] { "M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "date",
                        Error = ValidationErrorCode.InvalidFormat,
                        Message = $"Red {row}: 'date' ({dateRaw}) nije validan datum"
                    });
                    continue;
                }

                var dirRaw = csv.GetField("direction")?.Trim();
                if (!TryMapDirection(dirRaw, out var direction))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "direction",
                        Error = ValidationErrorCode.UnknownEnum,
                        Message = $"Red {row}: '{dirRaw}' nije validna vrednost za direction"
                    });
                    continue;
                }

                var amountRaw = csv.GetField("amount");
                if (!decimal.TryParse(
                        amountRaw,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var amount))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "amount",
                        Error = ValidationErrorCode.InvalidFormat,
                        Message = $"Red {row}: 'amount' ({amountRaw}) nije validan decimalni broj"
                    });
                    continue;
                }

                var currency = csv.GetField("currency");
                if (string.IsNullOrWhiteSpace(currency))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "currency",
                        Error = ValidationErrorCode.Required,
                        Message = $"Red {row}: 'currency' je obavezan"
                    });
                    continue;
                }

                var kindRaw = csv.GetField("kind")?.Trim();
                if (!TryMapKind(kindRaw, out var kind))
                {
                    errors.Add(new ValidationError
                    {
                        Tag = "kind",
                        Error = ValidationErrorCode.UnknownEnum,
                        Message = $"Red {row}: '{kindRaw}' nije validna vrednost za kind"
                    });
                    continue;
                }

                var beneficiary = csv.GetField("beneficiary-name")?.Trim() ?? string.Empty;
                var description = csv.GetField("description")?.Trim() ?? string.Empty;

                int? mcc = null;
                var mccRaw = csv.GetField("mcc");
                if (!string.IsNullOrWhiteSpace(mccRaw))
                {
                    if (int.TryParse(mccRaw, out var tmp))
                        mcc = tmp;
                    else
                        errors.Add(new ValidationError
                        {
                            Tag = "mcc",
                            Error = ValidationErrorCode.CheckDigitInvalid,
                            Message = $"Red {row}: 'mcc' ({mccRaw}) nije validan MCC kod"
                        });
                }

                if (errors.Any(e => e.Tag == "mcc" && e.Message.Contains($"Red {row}:")))
                    continue;
                validEntities.Add(new Transaction(
                    id,
                    beneficiary,
                    date,
                    direction,
                    amount,
                    description,
                    currency,
                    mcc,
                    kind
                ));
            }

            if (errors.Any())
                throw new CsvValidationException(errors);

            var existingIds = await _db.Transactions
                .AsNoTracking()
                .Select(t => t.Id)
                .ToListAsync();

            //var duplicateIds = validEntities
            //.Select(e => e.Id)
            //.Intersect(existingIds)
            //.ToList();
            var toAdd = validEntities
            .Where(e => !existingIds.Contains(e.Id))
            .ToList();

            if (!toAdd.Any())
                throw new BusinessException(
                    "no-new-transactions",
                    "Nema novih transakcija za import",
                    "Sve transakcije iz CSV fajla su već importovane."
                );
            //if (duplicateIds.Any())
            //    throw new BusinessException(
            //        "duplicate-transactions",
            //        $"Transakcije sa ID-jevima [{string.Join(", ", duplicateIds)}] su već importovane.",
            //        $"Obriši ili izmeni duplikate pre ponovnog importovanja."
            //    );
            await _db.Transactions.AddRangeAsync(toAdd);
            await _db.SaveChangesAsync();
        }

        private static bool TryMapDirection(string? value, out TransactionDirection dir)
        {
            switch (value?.ToLowerInvariant())
            {
                case "d": dir = TransactionDirection.Debit; return true;
                case "c": dir = TransactionDirection.Credit; return true;
                default: dir = default; return false;
            }
        }

        private static bool TryMapKind(string? value, out TransactionKind kind)
        {
            switch (value?.ToLowerInvariant())
            {
                case "dep": kind = TransactionKind.Deposit; return true;
                case "wdw": kind = TransactionKind.Withdrawal; return true;
                case "pmt": kind = TransactionKind.Payment; return true;
                case "fee": kind = TransactionKind.Fee; return true;
                case "inc": kind = TransactionKind.InterestCredit; return true;
                case "rev": kind = TransactionKind.Reversal; return true;
                case "adj": kind = TransactionKind.Adjustment; return true;
                case "lnd": kind = TransactionKind.LoanDisbursement; return true;
                case "lnr": kind = TransactionKind.LoanRepayment; return true;
                case "fcx": kind = TransactionKind.ForeignCurrencyExchange; return true;
                case "aop": kind = TransactionKind.AccountOpening; return true;
                case "acl": kind = TransactionKind.AccountClosing; return true;
                case "spl": kind = TransactionKind.SplitPayment; return true;
                case "sal": kind = TransactionKind.Salary; return true;
                default: kind = default; return false;
            }
        }
    }
}
