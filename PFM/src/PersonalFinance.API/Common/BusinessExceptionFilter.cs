using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonalFinance.Application.Common;

public class BusinessExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is BusinessException ex)
        {
            var problem = new BusinessProblemResponse
            {
                Problem = ex.Problem,
                Message = ex.Message,
                Details = ex.Details ?? string.Empty
            };
            switch (ex.Problem)
            {
                case "transaction-not-found":
                    context.HttpContext.Response.Headers["x-asee-problems"] =
                        JsonSerializer.Serialize(new[] { ex.Problem });
                    context.Result = new ObjectResult(problem) { StatusCode = 440 };
                    break;

                case "provided-category-does-not-exist":
                    context.HttpContext.Response.Headers["x-asee-problems"] =
                        JsonSerializer.Serialize(new[] { ex.Problem });
                    context.Result = new ObjectResult(problem) { StatusCode = 440 };
                    break;
                case "split-amount-over-transaction-amount":
                    context.HttpContext.Response.Headers["x-asee-problems"] =
                        JsonSerializer.Serialize(new[] { ex.Problem });
                    context.Result = new ObjectResult(problem) { StatusCode = 440 };
                    break;
                case "insufficient-splits":
                    context.HttpContext.Response.Headers["x-asee-problems"] =
                        JsonSerializer.Serialize(new[] { ex.Problem });
                    context.Result = new ObjectResult(problem) { StatusCode = 440 };
                    break;
                //case "duplicate-transactions":
                //    context.HttpContext.Response.Headers["x-asee-problems"] =
                //        JsonSerializer.Serialize(new[] { ex.Problem });
                //    context.Result = new ObjectResult(problem) { StatusCode = 440 };
                //    break;
                case "no-new-transactions":
                    context.HttpContext.Response.Headers["x-asee-problems"] =
                        JsonSerializer.Serialize(new[] { ex.Problem });
                    context.Result = new ObjectResult(problem) { StatusCode = 440 };
                    break;
                //case "task-already-claimed":
                //    context.HttpContext.Response.Headers["x-asee-problems"] =
                //        JsonSerializer.Serialize(new[] { ex.Problem });
                //    context.Result = new ObjectResult(problem)
                //    {
                //        StatusCode = StatusCodes.Status409Conflict
                //    }; break;
                default:
                    context.HttpContext.Response.Headers["x-asee-problems"] =
                        JsonSerializer.Serialize(new[] { ex.Problem });
                    context.Result = new ObjectResult(problem) { StatusCode = 400 };
                    break;

            }
            context.ExceptionHandled = true;
        }
    }
}
