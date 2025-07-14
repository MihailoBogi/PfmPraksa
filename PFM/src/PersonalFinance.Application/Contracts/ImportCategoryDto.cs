using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class ImportCategoryDto
    {
        public string Code { get; set; } = default;
        public string Name { get; set; } = default;
        public string ParentCode { get; set; }
    }
}
