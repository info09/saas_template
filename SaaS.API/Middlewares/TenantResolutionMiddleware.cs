using SaaS.Application.Interfaces;

namespace SaaS.API.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // TenantService is Scoped, resolving tenant for the current request context
        var tenantId = tenantService.GetCurrentTenantId();

        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Items["TenantId"] = tenantId;
        }
        else
        {
            // Require tenant id for multi-tenant endpoints.
            // This logic can be adjusted based on requirements (e.g., skip validation for /auth /catalog endpoints).
        }

        await _next(context);
    }
}
