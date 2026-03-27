using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services;

public class DatabaseMigrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task MigrateMasterAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var masterDbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

        _logger.LogInformation("Applying master database migrations...");
        await masterDbContext.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Master database migrations completed.");
    }

    public async Task MigrateTenantsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var masterDbContext = services.GetRequiredService<MasterDbContext>();
        var encryptionService = services.GetRequiredService<IEncryptionService>();

        _logger.LogInformation("Applying tenant database migrations...");

        var tenants = await masterDbContext.Tenants
            .Where(static tenant => tenant.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            try
            {
                var decryptedConnectionString = encryptionService.Decrypt(tenant.ConnectionString);
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                optionsBuilder.UseNpgsql(decryptedConnectionString);

                using var tenantDbContext = new TenantDbContext(optionsBuilder.Options);
                await tenantDbContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("Tenant database migrated: {TenantId}", tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tenant database migration failed: {TenantId}", tenant.Id);
                throw;
            }
        }

        _logger.LogInformation("Tenant database migrations completed.");
    }

    public async Task MigrateAllAsync(CancellationToken cancellationToken = default)
    {
        await MigrateMasterAsync(cancellationToken);
        await MigrateTenantsAsync(cancellationToken);
    }
}
