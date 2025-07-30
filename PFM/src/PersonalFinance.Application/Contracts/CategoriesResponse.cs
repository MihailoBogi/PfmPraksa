using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class CategoriesResponse
    {
        public List<CategoryDto> Items { get; set; } = new();
    }
}
