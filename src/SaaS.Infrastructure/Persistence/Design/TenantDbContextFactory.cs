using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SaaS.Infrastructure.Persistence.Design;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveApiProjectPath()
            ?? throw new InvalidOperationException("Could not locate the SaaS.API project directory.");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var masterConnectionString = configuration.GetConnectionString("MasterConnection")
            ?? "Host=localhost;Port=5433;Database=SaaS_MasterDb;Username=admin;Password=admin1234";

        var builder = new NpgsqlConnectionStringBuilder(masterConnectionString)
        {
            Database = "SaaS_Tenant_Template"
        };

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(builder.ConnectionString);

        return new TenantDbContext(optionsBuilder.Options);
    }

    private static string? ResolveApiProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(currentDirectory, "src", "SaaS.API"),
            Path.Combine(currentDirectory, "..", "SaaS.API"),
            Path.Combine(currentDirectory, "..", "..", "src", "SaaS.API")
        };

        foreach (var candidate in candidates.Select(Path.GetFullPath))
        {
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }
        }

        return null;
    }
}
