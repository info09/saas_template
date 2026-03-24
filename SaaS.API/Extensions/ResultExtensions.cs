using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Common.Models;

namespace SaaS.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
        => result.Succeeded ? new OkObjectResult(result) : new BadRequestObjectResult(result);

    public static ActionResult ToActionResult(this Result result)
        => result.Succeeded ? new OkResult() : new BadRequestObjectResult(result);
}
