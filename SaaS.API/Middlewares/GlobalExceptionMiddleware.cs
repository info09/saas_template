using Microsoft.Extensions.Logging;
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
            statusCode = HttpStatusCode.BadRequest;
            var validationErrors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(new 
            { 
                status = (int)statusCode,
                title = "One or more validation errors occurred.",
                errors = validationErrors 
            }));
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new 
        { 
            status = (int)statusCode,
            title = message,
            detail = exception.Message // In production, you might want to hide this detail
        });
        return context.Response.WriteAsync(result);
    }
}
