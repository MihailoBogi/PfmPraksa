using Microsoft.AspNetCore.Http;
using PersonalFinance.Application.Common.Pagination;
using PersonalFinance.Application.Contracts;
using System.Threading.Tasks;

namespace PersonalFinance.Application.Interfaces
{
    public interface ITransactionImporter
    {
        Task ImportAsync(IFormFile csvFile);
    }
}
