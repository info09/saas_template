using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyTenantMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TenantDbContext>>();
        var catalogContext = services.GetRequiredService<MasterDbContext>();
        var encryptionService = services.GetRequiredService<IEncryptionService>();

        logger.LogInformation("Starting automated tenant migrations...");

        var tenants = await catalogContext.Tenants.Where(t => t.IsActive).ToListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                logger.LogInformation("Migrating database for tenant: {TenantName} ({TenantId})", tenant.Name, tenant.Id);

                // 1. Decrypt connection string
                var decryptedConn = encryptionService.Decrypt(tenant.ConnectionString);

                // 2. Build options for TenantDbContext
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                optionsBuilder.UseNpgsql(decryptedConn);

                // 3. Create context instance
                // Note: We provide a dummy ITenantService because we are overriding the connection string via 'options'
                using var tenantContext = new TenantDbContext(optionsBuilder.Options, new DesignTimeTenantService());

                // 4. Run migrations
                await tenantContext.Database.MigrateAsync();

                logger.LogInformation("Successfully migrated database for tenant: {TenantId}", tenant.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error migrating database for tenant: {TenantId}", tenant.Id);
            }
        }

        logger.LogInformation("Finished automated tenant migrations.");
    }
}
