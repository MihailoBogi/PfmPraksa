using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Domain.Entities
{
    public class Category
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string? ParentCode { get; private set; }
        public Category? Parent { get; private set; }
        public ICollection<Category> Children { get; private set; } = new List<Category>();
        public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

        private Category() { }
        public Category(string code, string name, string? parentcode)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ParentCode = parentcode;
        }
        public void Update(string name, string? parentcode)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ParentCode = parentcode;
        }
    }
}
