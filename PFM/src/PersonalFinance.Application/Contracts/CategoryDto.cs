using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.Application.Contracts
{
    public class CategoryDto
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? ParentCode { get; set; }
    }
}
