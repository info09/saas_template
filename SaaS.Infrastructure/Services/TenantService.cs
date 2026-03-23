using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private string? _currentTenantId;
    private string? _connectionString;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        
        ResolveTenant();
    }

    public string? GetCurrentTenantId() => _currentTenantId;

    public string? GetConnectionString() => _connectionString;

    private void ResolveTenant()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Extract tenant ID from header
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
        {
            _currentTenantId = tenantIdValues.FirstOrDefault();
            
            if (!string.IsNullOrEmpty(_currentTenantId))
            {
                // We use a new scope to resolve CatalogDbContext to avoid circular dependency
                using var scope = _serviceProvider.CreateScope();
                var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
                
                var tenant = catalogDb.Tenants.FirstOrDefault(t => t.Id == _currentTenantId && t.IsActive);
                if (tenant != null)
                {
                    _connectionString = tenant.ConnectionString;
                }
                else
                {
                    // Tenant not found in Catalog DB
                    System.Diagnostics.Debug.WriteLine($"TenantService: Tenant '{_currentTenantId}' not found or inactive.");
                }
            }
        }
        else
        {
             System.Diagnostics.Debug.WriteLine("TenantService: Missing 'X-Tenant-Id' header in request.");
        }
    }
}
