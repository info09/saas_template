using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public interface ITenantProvisioningDbContextFactory
{
    TenantDbContext CreateDbContext(string connectionString);
}
