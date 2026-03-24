using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Persistence.Design
{
    internal class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            // Get directory of the API project to read appsettings.json
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SaaS.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<TenantDbContext>();

            // Use a sample tenant connection string for migrations. 
            // You can point this to a 'Template' or 'Dev' tenant database.
            // If not found, it will try to use a default or the catalog connection but with a different DB name.
            var connectionString = configuration.GetConnectionString("CatalogConnection")
                                 ?? "Host=localhost;Port=5433;Database=SaaS_Tenant_Template;Username=admin;Password=admin1234";

            builder.UseNpgsql(connectionString);

            return new TenantDbContext(builder.Options, new DesignTimeTenantService());
        }
    }
}

/// <summary>
/// Dummy service for design-time usage.
/// </summary>
internal class DesignTimeTenantService : ITenantService
{
    public string? GetConnectionString() => null;
    public string? GetCurrentTenantId() => null;
    public Task InitializeAsync() => Task.CompletedTask;
}