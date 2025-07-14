namespace PersonalFinance.Application.Contracts
{
    public class TransactionQuery
    {
        public List<string>? Kinds { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
    }
}
