using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class SingleCategorySplitDto
    {
        [Required]
        [JsonPropertyName("catcode")]
        public string CatCode { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
