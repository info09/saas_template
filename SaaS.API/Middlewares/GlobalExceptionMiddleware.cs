using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SaaS.Domain.Exceptions;
using System.Net;

namespace SaaS.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problemDetails;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(
                    failure => string.IsNullOrWhiteSpace(failure.PropertyName) ? "request" : failure.PropertyName,
                    failure => failure.ErrorMessage)
                .ToDictionary(group => group.Key, group => group.Distinct().ToArray());

            problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };
        }
        else if (exception is TenantNotFoundException tenantEx)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Tenant not found.",
                Detail = tenantEx.Message
            };
        }
        else if (exception is InvalidOperationException invalidOperationException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request.",
                Detail = invalidOperationException.Message
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The server could not complete the request."
            };
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
