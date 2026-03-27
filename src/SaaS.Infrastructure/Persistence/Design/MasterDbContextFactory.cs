using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SaaS.Infrastructure.Persistence.Design
{
    public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
    {
        public MasterDbContext CreateDbContext(string[] args)
        {
            var basePath = ResolveApiProjectPath();
            var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();

            if (!string.IsNullOrWhiteSpace(basePath))
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
}
