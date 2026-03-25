using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Interfaces;

namespace SaaS.API.Middlewares;

public class TokenVersionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenVersionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUserService currentUserService,
        ITokenVersionCacheService tokenVersionCacheService,
        IIdentityService identityService)
    {
        if (!currentUserService.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(currentUserService.UserId) ||
            string.IsNullOrWhiteSpace(currentUserService.TenantId) ||
            currentUserService.TokenVersion is null)
        {
            await WriteUnauthorizedAsync(context, "Invalid token claims.");
            return;
        }

        var cachedTokenVersion = await tokenVersionCacheService.GetTokenVersionAsync(
            currentUserService.TenantId,
            currentUserService.UserId,
            context.RequestAborted);

        if (cachedTokenVersion is null)
        {
            var dbTokenVersion = await identityService.GetTokenVersionAsync(currentUserService.UserId);
            if (dbTokenVersion is null)
            {
                await WriteUnauthorizedAsync(context, "The authenticated user no longer exists.");
                return;
            }

            cachedTokenVersion = dbTokenVersion.Value;
            await tokenVersionCacheService.SetTokenVersionAsync(
                currentUserService.TenantId,
                currentUserService.UserId,
                cachedTokenVersion.Value,
                context.RequestAborted);
        }

        if (currentUserService.TokenVersion.Value != cachedTokenVersion.Value)
        {
            await WriteUnauthorizedAsync(context, "This access token has been revoked.");
            return;
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized.",
            Detail = detail,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        });
    }
}
