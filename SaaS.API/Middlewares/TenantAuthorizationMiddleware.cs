using Microsoft.AspNetCore.Mvc;
using SaaS.Application.Interfaces;

namespace SaaS.API.Middlewares;

public class TenantAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService, ITenantService tenantService)
    {
        if (IsHealthEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (currentUserService.IsAuthenticated)
        {
            var headerTenantId = tenantService.GetCurrentTenantId();
            var claimTenantId = currentUserService.TenantId;

            if (!string.IsNullOrEmpty(headerTenantId) && !string.IsNullOrEmpty(claimTenantId))
            {
                if (headerTenantId != claimTenantId)
                {
                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Cross-tenant access denied.",
                        Detail = "The tenant specified in the header does not match your authenticated tenant claim."
                    });

                    return;
                }
            }
        }

        await _next(context);
    }

    private static bool IsHealthEndpoint(PathString path)
    {
        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
    }
}
