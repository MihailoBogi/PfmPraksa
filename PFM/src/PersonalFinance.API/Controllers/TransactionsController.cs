using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.API.Controllers
{
    [ApiController]
    [Route("")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionImporter _importer;
        private readonly ITransactionService _service;
        private readonly ISplitService _splitService;
        private readonly IAutoCategorizationService _autoCategorizationService;

        public TransactionsController(ITransactionImporter importer, ITransactionService service, ISplitService splitService, IAutoCategorizationService autoCategorizationService)
        {
            _importer = importer;
            _service = service;
            _splitService = splitService;
            _autoCategorizationService = autoCategorizationService;
        }

        [HttpPost("transactions/import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BusinessProblemResponse), 440)]
       // [ProducesResponseType(typeof(BusinessProblemResponse), 400)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessException(
                    problem: "file-missing",
                    message: "CSV fajl je obavezan",
                    details: "Neophodno je poslati fajl pod imenom 'file' u form-data body-u."
                );

            await _importer.ImportAsync(file);
            return Ok();
        }
        [HttpGet("transactions")]
        public async Task<IActionResult> Get(
                [FromQuery(Name = "transaction-kind")] List<string>? kinds,
                [FromQuery(Name = "start-date")][DataType(DataType.Date)] DateOnly? startDate,
                [FromQuery(Name = "end-date")][DataType(DataType.Date)] DateOnly? endDate,
                [FromQuery(Name = "page")] int page = 1,
                [FromQuery(Name = "page-size")][Range(1, 100)] int pageSize = 10,
                [FromQuery(Name = "sort-by")] string? sortBy = null,
                [FromQuery(Name = "sort-order")][RegularExpression("asc|desc")] string sortOrder = "asc")
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

        [HttpPost("transaction/{id}/categorize")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
       // [ProducesResponseType(440)]
        [ProducesResponseType(typeof(BusinessProblemResponse), 440)]
        public async Task<IActionResult> Categorize([FromRoute] int id, [FromBody] TransactionCategorizeCommand cmd)
        {
            if (!ModelState.IsValid) return BadRequest(new ValidationErrorResponse { 
                Errors = ModelStateErrors(ModelState) });

            await _service.CategorizeAsync(id, cmd.CatCode);
            return Ok();
        }
        [HttpPost("transaction/{id}/split")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        [ProducesResponseType(typeof(BusinessProblemResponse), 440)]
  //      [ProducesResponseType(440)]
        public async Task<IActionResult> Split([FromRoute] int id, [FromBody] SplitTransactionCommand cmd)
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ValidationErrorResponse
                    {
                        Errors = ModelStateErrors(ModelState)
                    });
            await _splitService.SplitAsync(id, cmd.Splits);
            return Ok();
        }
        [HttpPost("transaction/auto-categorize")]
        [ProducesResponseType(typeof(AutoCategorizationResultDto), 200)]
        public async Task<IActionResult> AutoCategorize()
        {
            var dto = await _autoCategorizationService.AutoCategorizeAsync();
            return Ok(dto);
        }
        private static List<ValidationError> ModelStateErrors(ModelStateDictionary ms) =>
        ms.Where(kvp => kvp.Value.Errors.Any())
          .SelectMany(kvp => kvp.Value.Errors
            .Select(err => new ValidationError
            {
                Tag = kvp.Key,
                Error = ValidationErrorCode.InvalidFormat,
                Message = err.ErrorMessage
            }))
          .ToList();      
    }
}
