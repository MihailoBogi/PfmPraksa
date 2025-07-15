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
            // 1) HTTP status 440 (može i 422 ili neki drugi, ali OAS3 hint-uje custom)
            context.HttpContext.Response.StatusCode = 440;

            // 2) Header x-asee sa listom problema
            //    Ovde šaljemo niz s jednim elementom; ako je više problema, možeš i za svaki
            var headerProblems = new[]
            {
                new {
                    L = ex.Problem,
                    C = context.HttpContext.Request.Path.Value?.Split('/').Last(), // ili neki drugi c
                    M = ex.Message
                }
            };
            var headerJson = JsonSerializer.Serialize(headerProblems);
            context.HttpContext.Response.Headers["x-asee"] = headerJson;

            // 3) Telo odgovora sa problem, message, details
            var body = new BusinessProblemResponse
            {
                Problem = ex.Problem,
                Message = ex.Message,
                Details = ex.Details
            };

            context.Result = new JsonResult(body)
            {
                StatusCode = 440,
                ContentType = "application/json"
            };

            context.ExceptionHandled = true;
        }
    }
}
