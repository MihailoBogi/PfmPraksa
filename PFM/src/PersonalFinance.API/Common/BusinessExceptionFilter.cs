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
                // add the x-asee-problems header
                context.HttpContext.Response.Headers["x-asee-problems"] = new[] { be.Problem };

                // prepare the response body
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
