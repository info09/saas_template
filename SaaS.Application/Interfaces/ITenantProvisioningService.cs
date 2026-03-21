using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface ITenantProvisioningService
{
    Task ProvisionTenantAsync(Tenant tenant, string adminEmail, string adminPassword, CancellationToken cancellationToken);
}
