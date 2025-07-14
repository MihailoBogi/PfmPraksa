using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonalFinance.Application.Common;

namespace PersonalFinance.API.Common.Filters
{
    public class CsvValidationFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is CsvValidationException cve)
            {
                var resp = new ValidationErrorResponse { Errors = cve.Errors };
                context.Result = new BadRequestObjectResult(resp);
                context.ExceptionHandled = true;
            }
        }
    }
}
