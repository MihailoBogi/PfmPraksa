using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.API.Common;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Common.Pagination;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using System.Threading.Tasks;

namespace PersonalFinance.API.Controllers
{
    [ApiController]
    [Route("transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionImporter _importer;
        private readonly ITransactionService _service;

        public TransactionsController(ITransactionImporter importer, ITransactionService service)
        {
            _importer = importer;
            _service = service;
        }

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BusinessErrorResponse), 440)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            // poslovno pravilo: fajl je obavezan
            if (file == null || file.Length == 0)
                throw new BusinessException(
                    problem: "file-missing",
                    message: "CSV fajl je obavezan",
                    details: "Neophodno je poslati fajl pod imenom 'file' u form-data body-u."
                );

            await _importer.ImportAsync(file);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Get(
                [FromQuery(Name = "transaction-kind")] List<string>? kinds,
                [FromQuery(Name = "start-date")] DateOnly? startDate,
                [FromQuery(Name = "end-date")] DateOnly? endDate,
                [FromQuery(Name = "page")] int page = 1,
                [FromQuery(Name = "page-size")] int pageSize = 10,
                [FromQuery(Name = "sort-by")] string? sortBy = null,
                [FromQuery(Name = "sort-order")] string sortOrder = "asc")
        {
            var query = new TransactionQuery
            {
                Kinds = kinds,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };
            var result = await _service.GetPagedAsync(query);
            return Ok(result);
        }
    }
}
