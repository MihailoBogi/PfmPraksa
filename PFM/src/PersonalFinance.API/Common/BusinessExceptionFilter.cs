using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonalFinance.Application.Common;

namespace PersonalFinance.API.Common.Filters
{
    public class BusinessExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is BusinessException be)
            {
                context.HttpContext.Response.Headers["x-asee-problems"] = new[] { be.Problem };

                var body = new BusinessErrorResponse
                {
                    Problem = be.Problem,
                    Message = be.Message,
                    Details = be.Details
                };

                context.Result = new ObjectResult(body)
                {
                    StatusCode = 440
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
