using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SaaS.Application.Common.Models;

namespace SaaS.API.Common.Filters;

public class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not Result<object>)
        {
            // If it's already a Result, don't wrap it again
            var type = objectResult.Value?.GetType();
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            {
                await next();
                return;
            }

            var wrappedResult = new Result<object>
            {
                Succeeded = true,
                Data = objectResult.Value,
                Message = "Success"
            };

            objectResult.Value = wrappedResult;
        }
        else if (context.Result is EmptyResult || (context.Result is StatusCodeResult sc && sc.StatusCode == 204))
        {
            context.Result = new ObjectResult(Result.Success("Success"))
            {
                StatusCode = 200 // Or keep 204 if you want, but the user wants a wrapper
            };
        }

        await next();
    }
}
