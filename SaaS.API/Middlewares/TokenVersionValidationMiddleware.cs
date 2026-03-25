using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SaaS.API.Options;
using SaaS.Application.Interfaces;

namespace SaaS.API.Middlewares;

public class TokenVersionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenVersionValidationMiddleware> _logger;
    private readonly TokenVersionValidationOptions _options;

    public TokenVersionValidationMiddleware(
        RequestDelegate next,
        ILogger<TokenVersionValidationMiddleware> logger,
        IOptions<TokenVersionValidationOptions> options)
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

        var tokenVersionCacheService = context.RequestServices.GetRequiredService<ITokenVersionCacheService>();
        var identityService = context.RequestServices.GetRequiredService<IIdentityService>();

        if (string.IsNullOrWhiteSpace(currentUserService.UserId) ||
            string.IsNullOrWhiteSpace(currentUserService.TenantId) ||
            currentUserService.SessionId is null ||
            currentUserService.TokenVersion is null)
        {
            _logger.LogWarning("Authenticated request {TraceId} is missing token/session claims.", context.TraceIdentifier);
            await WriteUnauthorizedAsync(context, "Invalid token claims.");
            return;
        }

        var cacheLookup = await tokenVersionCacheService.GetTokenVersionAsync(
            currentUserService.TenantId,
            currentUserService.UserId,
            context.RequestAborted);

        if (cacheLookup.Status == TokenVersionCacheStatus.Hit)
        {
            if (currentUserService.TokenVersion.Value != cacheLookup.TokenVersion)
            {
                _logger.LogWarning(
                    "Revoked access token detected from cache for tenant {TenantId}, user {UserId}, trace {TraceId}. ClaimVersion={ClaimVersion}, CacheVersion={CacheVersion}",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    context.TraceIdentifier,
                    currentUserService.TokenVersion.Value,
                    cacheLookup.TokenVersion);

                await WriteUnauthorizedAsync(context, "This access token has been revoked.");
                return;
            }
        }
        else
        {
            int? dbTokenVersion;

            try
            {
                dbTokenVersion = await identityService.GetTokenVersionAsync(currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Token version database fallback failed for tenant {TenantId}, user {UserId}, trace {TraceId}. CacheStatus={CacheStatus}",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    context.TraceIdentifier,
                    cacheLookup.Status);

                if (_options.FailOpenOnStateUnavailability)
                {
                    _logger.LogWarning(
                        "Fail-open is enabled. Allowing request {TraceId} despite unavailable token version state.",
                        context.TraceIdentifier);

                    await _next(context);
                    return;
                }

                await WriteServiceUnavailableAsync(context, "Authorization state is temporarily unavailable.");
                return;
            }

            if (dbTokenVersion is null)
            {
                _logger.LogWarning(
                    "Authenticated user not found during token version validation for tenant {TenantId}, user {UserId}, trace {TraceId}.",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    context.TraceIdentifier);

                await WriteUnauthorizedAsync(context, "The authenticated user no longer exists.");
                return;
            }

            if (cacheLookup.Status == TokenVersionCacheStatus.Miss)
            {
                _logger.LogInformation(
                    "Token version cache miss for tenant {TenantId}, user {UserId}. Database fallback succeeded for trace {TraceId}.",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    context.TraceIdentifier);
            }
            else if (cacheLookup.Status == TokenVersionCacheStatus.Unavailable)
            {
                _logger.LogWarning(
                    "Token version cache unavailable for tenant {TenantId}, user {UserId}. Database fallback succeeded for trace {TraceId}.",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    context.TraceIdentifier);
            }

            _ = await tokenVersionCacheService.SetTokenVersionAsync(
                currentUserService.TenantId,
                currentUserService.UserId,
                dbTokenVersion.Value,
                context.RequestAborted);

            if (currentUserService.TokenVersion.Value != dbTokenVersion.Value)
            {
                _logger.LogWarning(
                    "Revoked access token detected from database fallback for tenant {TenantId}, user {UserId}, trace {TraceId}. ClaimVersion={ClaimVersion}, DbVersion={DbVersion}",
                    currentUserService.TenantId,
                    currentUserService.UserId,
                    context.TraceIdentifier,
                    currentUserService.TokenVersion.Value,
                    dbTokenVersion.Value);

                await WriteUnauthorizedAsync(context, "This access token has been revoked.");
                return;
            }
        }

        bool? isSessionActive;

        try
        {
            isSessionActive = await identityService.IsSessionActiveAsync(
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

        if (isSessionActive is null)
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

        if (isSessionActive.Value == false)
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
