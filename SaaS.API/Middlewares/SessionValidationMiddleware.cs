using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SaaS.API.Options;
using SaaS.Application.Interfaces;

namespace SaaS.API.Middlewares;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;
    private readonly SessionValidationOptions _options;

    public SessionValidationMiddleware(
        RequestDelegate next,
        ILogger<SessionValidationMiddleware> logger,
        IOptions<SessionValidationOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUserService currentUserService)
    {
        if (IsHealthEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!currentUserService.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var sessionCacheService = context.RequestServices.GetRequiredService<ISessionCacheService>();
        var identityService = context.RequestServices.GetRequiredService<IIdentityService>();

        if (string.IsNullOrWhiteSpace(currentUserService.UserId) ||
            string.IsNullOrWhiteSpace(currentUserService.TenantId) ||
            currentUserService.SessionId is null)
        {
            _logger.LogWarning("Authenticated request {TraceId} is missing required session claims.", context.TraceIdentifier);
            await WriteUnauthorizedAsync(context, "Invalid token claims.");
            return;
        }

        var cacheLookup = await sessionCacheService.GetSessionAsync(
            currentUserService.TenantId,
            currentUserService.SessionId.Value,
            context.RequestAborted);

        if (cacheLookup.Status == SessionCacheStatus.Hit)
        {
            if (cacheLookup.Session is null ||
                cacheLookup.Session.UserId != currentUserService.UserId ||
                cacheLookup.Session.IsRevoked ||
                cacheLookup.Session.ExpiresAtUtc <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Revoked, expired, or mismatched session detected from cache for tenant {TenantId}, user {UserId}, session {SessionId}, trace {TraceId}.",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    currentUserService.SessionId.Value,
                    context.TraceIdentifier);

                await WriteUnauthorizedAsync(context, "This session has been revoked.");
                return;
            }

            await _next(context);
            return;
        }

        SessionCacheEntry? sessionState;

        try
        {
            sessionState = await identityService.GetSessionStateAsync(
                currentUserService.UserId,
                currentUserService.SessionId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Session validation failed for tenant {TenantId}, user {UserId}, session {SessionId}, trace {TraceId}.",
                currentUserService.TenantId,
                currentUserService.UserId,
                currentUserService.SessionId.Value,
                context.TraceIdentifier);

            if (_options.FailOpenOnStateUnavailability)
            {
                _logger.LogWarning(
                    "Fail-open is enabled. Allowing request {TraceId} despite unavailable session state.",
                    context.TraceIdentifier);

                await _next(context);
                return;
            }

            await WriteServiceUnavailableAsync(context, "Authorization session state is temporarily unavailable.");
            return;
        }

        if (sessionState is null)
        {
            _logger.LogWarning(
                "Authenticated session not found for tenant {TenantId}, user {UserId}, session {SessionId}, trace {TraceId}.",
                currentUserService.TenantId,
                currentUserService.UserId,
                currentUserService.SessionId.Value,
                context.TraceIdentifier);

            await WriteUnauthorizedAsync(context, "The authenticated session no longer exists.");
            return;
        }

        if (sessionState.IsRevoked || sessionState.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Revoked or expired session detected for tenant {TenantId}, user {UserId}, session {SessionId}, trace {TraceId}.",
                currentUserService.TenantId,
                currentUserService.UserId,
                currentUserService.SessionId.Value,
                context.TraceIdentifier);

            await WriteUnauthorizedAsync(context, "This session has been revoked.");
            return;
        }

        if (cacheLookup.Status == SessionCacheStatus.Miss)
        {
            _logger.LogInformation(
                "Session cache miss for tenant {TenantId}, user {UserId}, session {SessionId}. Database fallback succeeded for trace {TraceId}.",
                currentUserService.TenantId,
                currentUserService.UserId,
                currentUserService.SessionId.Value,
                context.TraceIdentifier);
        }
        else if (cacheLookup.Status == SessionCacheStatus.Unavailable)
        {
            _logger.LogWarning(
                "Session cache unavailable for tenant {TenantId}, user {UserId}, session {SessionId}. Database fallback succeeded for trace {TraceId}.",
                currentUserService.TenantId,
                currentUserService.UserId,
                currentUserService.SessionId.Value,
                context.TraceIdentifier);
        }

        _ = await sessionCacheService.SetSessionAsync(
            currentUserService.TenantId,
            currentUserService.SessionId.Value,
            sessionState,
            context.RequestAborted);

        await _next(context);
    }

    private static bool IsHealthEndpoint(PathString path)
    {
        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
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

    private static async Task WriteServiceUnavailableAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Service unavailable.",
            Detail = detail,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        });
    }
}
