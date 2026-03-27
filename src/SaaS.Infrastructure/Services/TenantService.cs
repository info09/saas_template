using Microsoft.AspNetCore.Http;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MasterDbContext _masterDbContext;
    private readonly IEncryptionService _encryptionService;
    private string? _currentTenantId;
    private string? _connectionString;
    private bool _isResolved;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        MasterDbContext masterDbContext,
        IEncryptionService encryptionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _masterDbContext = masterDbContext;
        _encryptionService = encryptionService;
    }

    public string? GetCurrentTenantId()
    {
        EnsureResolved();
        return _currentTenantId;
    }

    public string? GetConnectionString()
    {
        EnsureResolved();
        return _connectionString;
    }

    private void EnsureResolved()
    {
        if (_isResolved)
        {
            return;
        }

        _isResolved = true;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
        {
            _currentTenantId = tenantIdValues.FirstOrDefault();

            if (!string.IsNullOrEmpty(_currentTenantId))
            {
                var tenant = _masterDbContext.Tenants.FirstOrDefault(t => t.Id == _currentTenantId && t.IsActive);
                if (tenant != null)
                {
                    _connectionString = _encryptionService.Decrypt(tenant.ConnectionString);
                }
            }
        }
    }
}
