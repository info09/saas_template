using Microsoft.EntityFrameworkCore;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public class TenantSchemaMigrator : ITenantSchemaMigrator
{
    public Task MigrateAsync(TenantDbContext tenantDbContext, CancellationToken cancellationToken)
    {
        return tenantDbContext.Database.MigrateAsync(cancellationToken);
    }
}
