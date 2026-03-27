using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Common.Models;

namespace SaaS.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.Succeeded)
        {
            return new OkObjectResult(result);
        }

        return CreateProblemResult(result.Errors, result.Message);
    }

    public static ActionResult ToActionResult(this Result result)
    {
        if (result.Succeeded)
        {
            return string.IsNullOrWhiteSpace(result.Message)
                ? new OkResult()
                : new OkObjectResult(Result.Success(result.Message));
        }

        return CreateProblemResult(result.Errors, result.Message);
    }

    private static ObjectResult CreateProblemResult(IEnumerable<string>? errors, string? message)
    {
        var errorList = errors?
            .Where(static error => !string.IsNullOrWhiteSpace(error))
            .ToArray() ?? [];

        if (errorList.Length == 0)
        {
            return new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Request failed.",
                Detail = message ?? "The request could not be completed."
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        if (errorList.Length == 1)
        {
            return new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Request failed.",
                Detail = errorList[0]
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var validationProblem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["errors"] = errorList
        })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = message
        };

        return new ObjectResult(validationProblem)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }
}
