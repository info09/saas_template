using Microsoft.EntityFrameworkCore;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly MasterDbContext _context;

    public TenantRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();
        return await _context.Tenants.AnyAsync(tenant => tenant.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }
}
