using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class RuleResultDto
    {
        public string CatCode { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int CountMatched { get; set; }
    }

    public class AutoCategorizationResultDto
    {
        public int TotalCategorized { get; set; }
        public List<RuleResultDto> RuleResults { get; set; } = new();
    }
}
