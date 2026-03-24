using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface ITenantRepository
{
    IQueryable<Tenant> Queryable();
    Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken);
}
