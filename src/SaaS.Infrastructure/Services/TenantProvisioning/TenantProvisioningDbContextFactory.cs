using Microsoft.EntityFrameworkCore;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public class TenantProvisioningDbContextFactory : ITenantProvisioningDbContextFactory
{
    public TenantDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.EnableRetryOnFailure());

        return new TenantDbContext(optionsBuilder.Options);
    }
}
