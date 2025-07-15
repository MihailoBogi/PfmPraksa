using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;

namespace PersonalFinance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpendingAnalyticsController : ControllerBase
    {
        private readonly ITransactionService _service;
        public SpendingAnalyticsController(ITransactionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SpendingByCategoryResponse), 200)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        public async Task<IActionResult> Get([FromQuery(Name = "catcode")] string? catcode,
                                         [FromQuery(Name = "start-date")] DateTime? start,
                                         [FromQuery(Name = "end-date")] DateTime? end,
                                         [FromQuery(Name = "direction")] string? dir)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationErrorResponse { Errors = ModelStateErrors(ModelState) });

            var q = new SpendingAnalyticsQuery
            {
                CategoryCode = catcode,
                StartDate = start is null ? null : DateOnly.FromDateTime(start.Value),
                EndDate = end is null ? null : DateOnly.FromDateTime(end.Value),
                Direction = dir
            };

            var result = await _service.GetSpendingsByCategoryAsync(q);
            return Ok(result);
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
