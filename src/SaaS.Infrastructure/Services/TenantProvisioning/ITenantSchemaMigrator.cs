using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public interface ITenantSchemaMigrator
{
    Task MigrateAsync(TenantDbContext tenantDbContext, CancellationToken cancellationToken);
}
