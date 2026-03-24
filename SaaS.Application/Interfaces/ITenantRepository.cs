using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken);
}
