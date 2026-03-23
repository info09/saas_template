using SaaS.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace SaaS.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";

        if (exception is TenantNotFoundException tenantEx)
        {
            statusCode = HttpStatusCode.NotFound;
            message = tenantEx.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = HttpStatusCode.Forbidden;
            message = "You do not have permission to access this resource.";
        }
        else if (exception is FluentValidation.ValidationException validationEx)
        {
            var validationErrors = validationEx.Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .ToList();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var result = SaaS.Application.Common.Models.Result.Failure(
                validationErrors,
                "One or more validation errors occurred.");

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = SaaS.Application.Common.Models.Result.Failure(
            new[] { exception.Message },
            message);

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
