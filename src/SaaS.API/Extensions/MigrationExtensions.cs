using SaaS.Infrastructure.Services;

namespace SaaS.API.Extensions;

public static class MigrationExtensions
{
    public static async Task<bool> RunMigrationCommandAsync(this WebApplication app, string[] args)
    {
        if (!args.Contains("--migrate-master", StringComparer.OrdinalIgnoreCase) &&
            !args.Contains("--migrate-tenants", StringComparer.OrdinalIgnoreCase) &&
            !args.Contains("--migrate-all", StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        using var scope = app.Services.CreateScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();

        if (args.Contains("--migrate-all", StringComparer.OrdinalIgnoreCase))
        {
            await migrationService.MigrateAllAsync();
            return true;
        }

        if (args.Contains("--migrate-master", StringComparer.OrdinalIgnoreCase))
        {
            await migrationService.MigrateMasterAsync();
        }

        if (args.Contains("--migrate-tenants", StringComparer.OrdinalIgnoreCase))
        {
            await migrationService.MigrateTenantsAsync();
        }

        return true;
    }
}
