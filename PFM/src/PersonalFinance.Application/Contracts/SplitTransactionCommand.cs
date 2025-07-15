using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class SplitTransactionCommand
    {
        [Required]
        [JsonPropertyName("splits")]
        public List<SingleCategorySplitDto> Splits { get; set; } = new();
    }
}
