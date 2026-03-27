using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Tenants.Services;

public class TenantOnboardingService : ITenantOnboardingService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantProvisioningService _tenantProvisioningService;

    public TenantOnboardingService(ITenantRepository tenantRepository, ITenantProvisioningService tenantProvisioningService)
    {
        _tenantRepository = tenantRepository;
        _tenantProvisioningService = tenantProvisioningService;
    }

    public async Task<Result<string>> CreateTenantAsync(string name, string adminEmail, string adminPassword, CancellationToken cancellationToken)
    {
        var tenantName = name.Trim();
        var tenantAlreadyExists = await _tenantRepository.ExistsByNameAsync(tenantName, cancellationToken);
        if (tenantAlreadyExists)
        {
            return Result<string>.Failure("Tenant is already exists with the same name. Please choose a different name.");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = tenantName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tenantProvisioningService.ProvisionTenantAsync(tenant, adminEmail, adminPassword, cancellationToken);
        await _tenantRepository.CreateTenantAsync(tenant, cancellationToken);

        return Result<string>.Success(tenant.Id);
    }
}
