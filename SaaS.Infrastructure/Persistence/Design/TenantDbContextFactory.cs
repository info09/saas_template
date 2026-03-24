using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SaaS.Infrastructure.Persistence.Design
{
    internal class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SaaS.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<TenantDbContext>();

            var connectionString = configuration.GetConnectionString("MasterConnection")
                                 ?? "Host=localhost;Port=5433;Database=SaaS_MasterDb;Username=admin;Password=admin1234";

            builder.UseNpgsql(connectionString);

            return new TenantDbContext(builder.Options);
        }
    }
}
