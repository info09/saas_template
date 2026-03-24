using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SaaS.Infrastructure.Persistence.Design
{
    public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
    {
        public MasterDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SaaS.API");
            var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();

            if (Directory.Exists(basePath))
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString = configuration.GetConnectionString("MasterConnection")
                    ?? "Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password";

                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password");
            }

            return new MasterDbContext(optionsBuilder.Options);
        }
    }
}
