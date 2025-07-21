using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Infrastructure.Services
{
    public class AutoCategorizationService : IAutoCategorizationService
    {
        private readonly ApplicationDbContext _db;
        private readonly AutoCategorizationOptions _opts;

        public AutoCategorizationService(
            ApplicationDbContext db,
            IOptions<AutoCategorizationOptions> opts
        )
        {
            _db = db;
            _opts = opts.Value;
            Console.WriteLine($"[AutoCat] Loaded {_opts.Rules.Count} rules");
        }

        public async Task<AutoCategorizationResultDto> AutoCategorizeAsync()
        {
            var result = new AutoCategorizationResultDto();

            foreach (var rule in _opts.Rules)
            {
                // 1. koliki je broj novih kandidata za ovo pravilo?
                var sqlCount = $@"
                    SELECT COUNT(*) 
                      FROM ""Transactions""
                     WHERE {rule.Predicate}
                       AND ""CatCode"" IS NULL";

                var matchCount = await _db
                    .Database
                    .ExecuteSqlRawAsync($"SELECT 0"); // workaround, izbrojaćemo drugačije

                // bolji način: direktno count uz FromSqlRaw
                var cnt = await _db
                    .Transactions
                    .FromSqlRaw(sqlCount)
                    .CountAsync();

                if (cnt == 0)
                    continue;

                // 2. ažuriraj tim transakcijama catcode
                var sqlUpdate = $@"
                    UPDATE ""Transactions""
                       SET ""CatCode"" = '{{0}}'
                     WHERE {rule.Predicate}
                       AND ""CatCode"" IS NULL";

                var affected = await _db
                    .Database
                    .ExecuteSqlRawAsync(
                        string.Format(sqlUpdate, rule.CatCode)
                    );

                result.RuleResults.Add(new RuleResultDto
                {
                    CatCode = rule.CatCode,
                    Description = rule.Description,
                    CountMatched = affected
                });
                result.TotalCategorized += affected;
            }

            return result;
        }
    }
}
