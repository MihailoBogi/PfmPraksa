using System.Text.Json.Serialization;

namespace PersonalFinance.Application.Contracts
{
    public class TransactionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("beneficiary-name")]
        public string? BeneficiaryName { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; } = default!;

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = default!;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = default!;

        [JsonPropertyName("mcc")]
        public int? Mcc { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = default!;

        [JsonPropertyName("catcode")]
        public string? CatCode { get; set; }
        public List<SingleCategorySplitDto> Splits { get; set; } = new();
    }
}
