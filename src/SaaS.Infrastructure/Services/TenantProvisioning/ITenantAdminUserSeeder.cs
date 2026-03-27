using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public interface ITenantAdminUserSeeder
{
    Task SeedAsync(TenantDbContext tenantDbContext, string adminEmail, string adminPassword, CancellationToken cancellationToken);
}
