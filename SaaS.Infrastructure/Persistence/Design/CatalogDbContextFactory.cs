using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SaaS.Infrastructure.Persistence.Design;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SaaS.API");
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        
        if (Directory.Exists(basePath))
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("CatalogConnection") 
                ?? "Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password";

            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=SaaS_MasterDb;Username=postgres;Password=your_password");
        }

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
