using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Contracts
{
    public class SpendingGroupDto
    {
        [JsonPropertyName("catcode")]
        public string? CatCode { get; set; } = default;
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; } = default;
        [JsonPropertyName("count")]
        public int Count { get; set; } = default;
    }
    public class SpendingByCategoryResponse
    {
        [JsonPropertyName("groups")]
        public List<SpendingGroupDto> Groups { get; set; } = new();
    }
}
