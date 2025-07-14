using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Common.Pagination;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalFinance.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _db;
        public TransactionService(ApplicationDbContext db) => _db = db;

        public async Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionQuery q)
        {
            // 1) Base query
            var baseQ = _db.Transactions.AsNoTracking().AsQueryable();

            // 2) Period filter
            if (q.StartDate.HasValue)
                baseQ = baseQ.Where(t => t.Date >= q.StartDate.Value.ToDateTime(TimeOnly.MinValue));
            if (q.EndDate.HasValue)
                baseQ = baseQ.Where(t => t.Date <= q.EndDate.Value.ToDateTime(TimeOnly.MaxValue));

            // 3) Kind filter (parsiraj stringove u enum, pa filter)
            if (q.Kinds?.Any() == true)
            {
                var allowedKinds = q.Kinds
                    .Select(k => k.Trim().ToLowerInvariant())
                    .Select(k => k switch
                    {
                        "dep" => TransactionKind.Deposit,
                        "wdw" => TransactionKind.Withdrawal,
                        "pmt" => TransactionKind.Payment,
                        "fee" => TransactionKind.Fee,
                        "inc" => TransactionKind.InterestCredit,
                        "rev" => TransactionKind.Reversal,
                        "adj" => TransactionKind.Adjustment,
                        "lnd" => TransactionKind.LoanDisbursement,
                        "lnr" => TransactionKind.LoanRepayment,
                        "fcx" => TransactionKind.ForeignCurrencyExchange,
                        "aop" => TransactionKind.AccountOpening,
                        "acl" => TransactionKind.AccountClosing,
                        "spl" => TransactionKind.SplitPayment,
                        "sal" => TransactionKind.Salary,
                        _ => throw new BusinessException(
                                     "invalid-kind",
                                     $"'{k}' nije validna vrednost za kind",
                                     null)
                    })
                    .ToList();

                baseQ = baseQ.Where(t => allowedKinds.Contains(t.Kind));
            }

            // 4) Total count (pre‐paging)
            var total = await baseQ.CountAsync();

            // 5) Sorting
            var desc = string.Equals(q.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            baseQ = (q.SortBy ?? "date").Trim().ToLowerInvariant() switch
            {
                "amount" => desc ? baseQ.OrderByDescending(t => t.Amount) : baseQ.OrderBy(t => t.Amount),
                "currency" => desc ? baseQ.OrderByDescending(t => t.Currency) : baseQ.OrderBy(t => t.Currency),
                "kind" => desc ? baseQ.OrderByDescending(t => t.Kind) : baseQ.OrderBy(t => t.Kind),
                "id" => desc ? baseQ.OrderByDescending(t => t.Id) : baseQ.OrderBy(t => t.Id),
                _ => desc ? baseQ.OrderByDescending(t => t.Date) : baseQ.OrderBy(t => t.Date),
            };

            // 6) Paging
            var skip = (q.Page - 1) * q.PageSize;
            var pageEntities = await baseQ
                .Skip(skip)
                .Take(q.PageSize)
                .ToListAsync();

            // 7) Mapiranje u DTO
            var items = pageEntities.Select(t => new TransactionDto
            {
                Id = t.Id.ToString(),
                BeneficiaryName = t.BeneficiaryName,
                Date = t.Date.ToString("yyyy-MM-dd"),
                Direction = t.Direction == TransactionDirection.Debit ? "d" : "c",
                Amount = t.Amount.ToString("F2"),
                Description = t.Description,
                Currency = t.Currency,
                Mcc = t.Mcc?.ToString(),
                Kind = t.Kind switch
                {
                    TransactionKind.Deposit => "dep",
                    TransactionKind.Withdrawal => "wdw",
                    TransactionKind.Payment => "pmt",
                    TransactionKind.Fee => "fee",
                    TransactionKind.InterestCredit => "inc",
                    TransactionKind.Reversal => "rev",
                    TransactionKind.Adjustment => "adj",
                    TransactionKind.LoanDisbursement => "lnd",
                    TransactionKind.LoanRepayment => "lnr",
                    TransactionKind.ForeignCurrencyExchange => "fcx",
                    TransactionKind.AccountOpening => "aop",
                    TransactionKind.AccountClosing => "acl",
                    TransactionKind.SplitPayment => "spl",
                    TransactionKind.Salary => "sal",
                    _ => string.Empty
                },
                CatCode = t.CatCode
            }).ToList();

            // 8) Vrati PagedResult preko object‐initializer
            return new PagedResult<TransactionDto>
            {
                TotalCount = total,
                PageSize = q.PageSize,
                Page = q.Page,
                Items = items
            };
        }
    }
}
