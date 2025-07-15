using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Domain.Entities
{
    public class Split
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public string CatCode { get; set; } = default!;
        public decimal Amount { get; set; }
        public Transaction Transaction { get; set; } = default!;
    }
}
