using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SaaS.Infrastructure.Persistence.Design;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SaaS.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var catalogConnectionString = configuration.GetConnectionString("MasterConnection")
            ?? "Host=localhost;Port=5433;Database=SaaS_MasterDb;Username=admin;Password=admin1234";

        var builder = new NpgsqlConnectionStringBuilder(catalogConnectionString)
        {
            Database = "SaaS_Tenant_Template"
        };

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(builder.ConnectionString);

        return new TenantDbContext(optionsBuilder.Options);
    }
}
