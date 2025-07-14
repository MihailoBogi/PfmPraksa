namespace PersonalFinance.Application.Common
{
    public class BusinessException : Exception
    {
        public string Problem { get; }
        public string Details { get; }

        public BusinessException(string problem, string message, string details = "")
            : base(message)
        {
            Problem = problem;
            Details = details;
        }
    }
}
