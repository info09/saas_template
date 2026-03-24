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
        if (currentUserService.IsAuthenticated)
        {
            var headerTenantId = tenantService.GetCurrentTenantId();
            var claimTenantId = currentUserService.TenantId;

            if (!string.IsNullOrEmpty(headerTenantId) && !string.IsNullOrEmpty(claimTenantId))
            {
                if (headerTenantId != claimTenantId)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = 403,
                        title = "Cross-tenant access denied.",
                        detail = "The tenant specified in the header does not match your authenticated tenant claim."
                    });
                    return;
                }
            }
        }
        await _next(context);
    }
}
