using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Common
{
    public class AutoCategorizationRule
    {
        public string Field { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string CatCode { get; set; } = default!;
        public string Predicate { get; set; } = default!;
    }
    public class AutoCategorizationOptions
    {
        public List<AutoCategorizationRule> Rules { get; set; } = new();
    }
}
