namespace PersonalFinance.Application.Contracts
{
    public class ImportTransactionDto
    {
        public string? Id { get; set; }

        public string BeneficiaryName { get; set; } = string.Empty;

        public string? Date { get; set; }

        public string? Direction { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; } = default!;

        public string Currency { get; set; } = default!;

        public int? Mcc { get; set; }

        public string? Kind { get; set; }
    }
}
