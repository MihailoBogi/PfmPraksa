using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class SpendingAnalyticsQuery
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CategoryCode { get; set; }
        public string? Direction { get; set; }
    }
}
