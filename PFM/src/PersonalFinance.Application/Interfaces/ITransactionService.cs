using Microsoft.AspNetCore.Http;
using PersonalFinance.Application.Common.Pagination;
using PersonalFinance.Application.Contracts;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionQuery query);
        Task CategorizeAsync(int transactionId, string catCode);
        Task<SpendingByCategoryResponse> GetSpendingsByCategoryAsync(SpendingAnalyticsQuery query);
        Task ClaimAsync(int transactionId, string userName);
    }
}
