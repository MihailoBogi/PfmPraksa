using CsvHelper.Configuration.Attributes;
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
            //clamping
            var page = q.Page < 1 ? 1 : q.Page;
            var pageSize = q.PageSize < 1 ? 1
                          : q.PageSize > 100 ? 100
                          : q.PageSize;

            var baseQ = _db.Transactions.AsNoTracking().Include(t => t.Splits).AsQueryable();

            if (q.StartDate.HasValue)
                baseQ = baseQ.Where(t => t.Date >= q.StartDate.Value.ToDateTime(TimeOnly.MinValue));
            if (q.EndDate.HasValue)
                baseQ = baseQ.Where(t => t.Date <= q.EndDate.Value.ToDateTime(TimeOnly.MaxValue));

            //  string u enum pa filter
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

            var total = await baseQ.CountAsync();//total count za paging

            var desc = string.Equals(q.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            baseQ = (q.SortBy ?? "date").Trim().ToLowerInvariant() switch
            {
                "amount" => desc ? baseQ.OrderByDescending(t => t.Amount) : baseQ.OrderBy(t => t.Amount),
                "currency" => desc ? baseQ.OrderByDescending(t => t.Currency) : baseQ.OrderBy(t => t.Currency),
                "kind" => desc ? baseQ.OrderByDescending(t => t.Kind) : baseQ.OrderBy(t => t.Kind),
                "id" => desc ? baseQ.OrderByDescending(t => t.Id) : baseQ.OrderBy(t => t.Id),
                _ => desc ? baseQ.OrderByDescending(t => t.Date) : baseQ.OrderBy(t => t.Date),
            };
            //ovo su main transakcije
            var skip = (page - 1) * pageSize;
            var pageEntities = await baseQ
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var items = pageEntities.Select(t => new TransactionDto
            {
                Id = t.Id.ToString(),
                BeneficiaryName = t.BeneficiaryName,
                Date = t.Date.ToString("yyyy-MM-dd"),
                Direction = t.Direction == TransactionDirection.Debit ? "d" : "c",
                Amount = t.Amount.ToString("F2"),
                Description = t.Description,
                Currency = t.Currency,
                Mcc = t.Mcc.HasValue
                ? (int?)t.Mcc.Value
                : null,
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
                CatCode = t.CatCode,
                Splits = t.Splits.Select(s => new SingleCategorySplitDto
                {
                    CatCode = s.CatCode,
                    Amount = s.Amount
                }).ToList()
            }).ToList();

            // vrati PagedResult preko object‐initializer
            return new PagedResult<TransactionDto>
            {
                TotalCount = total,
                PageSize = q.PageSize,
                Page = q.Page,
                Items = items
            };
        }
        public async Task CategorizeAsync(int transactionId, string catCode)
        {
            var tx = await _db.Transactions.FindAsync(transactionId)
             ?? throw new BusinessException(
                    "transaction-not-found",
                    "Transaction not found",
                    $"Transaction with ID {transactionId} does not exist");

            var cat = await _db.Categories.FindAsync(catCode)
                      ?? throw new BusinessException(
                             "provided-category-does-not-exist",
                             "Category not found",
                             $"Category with code '{catCode}' does not exist");

            tx.Categorize(catCode);      
                                          
            await _db.SaveChangesAsync();
        }
        public async Task<SpendingByCategoryResponse> GetSpendingsByCategoryAsync(SpendingAnalyticsQuery q)
        {
            var cat = string.IsNullOrWhiteSpace(q.CategoryCode)
              ? null
              : q.CategoryCode.Trim();

            var txs = _db.Transactions.AsNoTracking().AsQueryable();

            // filter po kat i potkat
            if (cat != null)
            {
                var subCats = await _db.Categories
                                       .Where(c => c.ParentCode == cat)
                                       .Select(c => c.Code)
                                       .ToListAsync();

                txs = txs.Where(t => t.CatCode == cat || subCats.Contains(t.CatCode!)
                );
            }

            // filet po date
            if (q.StartDate.HasValue)
                txs = txs.Where(t => t.Date >= q.StartDate.Value.ToDateTime(TimeOnly.MinValue));
            if (q.EndDate.HasValue)
                txs = txs.Where(t => t.Date <= q.EndDate.Value.ToDateTime(TimeOnly.MaxValue));

            // dir
            if (!string.IsNullOrWhiteSpace(q.Direction))
            {
                var d = q.Direction.Trim().ToLowerInvariant();
                if (d != "d" && d != "c" && d != "debit" && d != "credit")
                    throw new BusinessException(
                        "invalid-format",
                        "Invalid direction",
                        $"Direction '{q.Direction}' is not valid");
                var dirEnum = (d == "d" || d == "debit")
                            ? TransactionDirection.Debit
                            : TransactionDirection.Credit;
                txs = txs.Where(t => t.Direction == dirEnum);
            }
            // ako nije proslednjena kategorija, grupise se po Parentu inace je t.Cat
            var groups = await txs
                .GroupBy(t => cat == null
                    ? _db.Categories
                         .Where(c => c.Code == t.CatCode)
                         .Select(c => c.ParentCode ?? c.Code)
                         .FirstOrDefault()!
                    : t.CatCode!)
                .Select(g => new SpendingGroupDto
                {
                    CatCode = g.Key!,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .ToListAsync();

            return new SpendingByCategoryResponse { Groups = groups };
        }
        public async Task ClaimAsync(int transactionId, string userName)
        {
            var tx = await _db.Transactions.FindAsync(transactionId)
            ?? throw new BusinessException(
                  "transaction-not-found",
                  "Transaction not found",
                  $"Transaction {transactionId} does not exist");

            throw new BusinessException(
                "task-already-claimed",
                "Task already claimed",
                $"User {userName} has claimed the task");
        }
    }
}
