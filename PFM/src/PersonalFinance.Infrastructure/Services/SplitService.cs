using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Infrastructure.Services
{
    public class SplitService : ISplitService
    {
        private readonly ApplicationDbContext _db;
        public SplitService(ApplicationDbContext db) => _db = db;

        public async Task SplitAsync(int transactionId, IEnumerable<SingleCategorySplitDto> splits)
        {
            var tx = await _db.Transactions
             .Include(t => t.Splits)
             .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (tx == null)
                throw new BusinessException(
                    "transaction-not-found",
                    "Transaction not found",
                    $"Transaction with ID {transactionId} does not exist");

            //foreach (var split in splits)
            //{
            //    if (!await _db.Categories.AnyAsync(c => c.Code == split.CatCode))
            //        throw new BusinessException(
            //            "provided-category-does-not-exist",
            //            "Category not found",
            //            $"Category with code '{split.CatCode}' does not exist");

            //    if(split.CatCode != tx.CatCode)
            //        throw new BusinessException(
            //            "split-category-invalid",
            //            "Split category is invalid",
            //            $"Category '{split.CatCode}' is not within original category '{tx.CatCode}'");
            //}
            foreach (var split in splits)
            {
                if (await _db.Categories.FindAsync(split.CatCode) == null)
                    throw new BusinessException(
                        "provided-category-does-not-exist",
                        "Category not found",
                        $"Category with code '{split.CatCode}' does not exist");
            }

            var totalSplit = splits.Sum(s => s.Amount);
            if (totalSplit > tx.Amount)
                throw new BusinessException(
                    "split-amount-over-transaction-amount",
                    "Split amount is larger then transaction amount",
                    $"Split amount ({totalSplit}) exceeds transaction amount ({tx.Amount})");

            if (tx.Splits.Any())
                _db.Splits.RemoveRange(tx.Splits);

            foreach (var s in splits)
            {
                tx.Splits.Add(new Split
                {
                    TransactionId = transactionId,
                    CatCode = s.CatCode,
                    Amount = s.Amount
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
