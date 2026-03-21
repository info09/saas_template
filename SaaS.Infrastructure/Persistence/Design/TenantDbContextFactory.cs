using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Persistence.Design;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        
        // This is purely for generating migrations via the CLI.
        // The real connection strings are injected at runtime via ITenantService.
        optionsBuilder.UseNpgsql("Host=localhost;Database=SaaS_TenantTemplateDb_DesignTime;Username=postgres;Password=your_password");

        return new TenantDbContext(optionsBuilder.Options, new DummyDesignTenantService());
    }

    private class DummyDesignTenantService : ITenantService
    {
        public string? GetCurrentTenantId() => "design_time_tenant";
        public string? GetConnectionString() => "Host=localhost;Database=SaaS_TenantTemplateDb_DesignTime;Username=postgres;Password=your_password";
    }
}
