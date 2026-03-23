using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Identity;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence;

/// <summary>
/// Tenant-specific database context.
/// </summary>
public class TenantDbContext : IdentityDbContext<AppUser>
{
    private readonly ITenantService _tenantService;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var tenantConnectionString = _tenantService.GetConnectionString();
        
        if (!string.IsNullOrEmpty(tenantConnectionString))
        {
            // Dynamically set connection string based on current tenant
            optionsBuilder.UseNpgsql(tenantConnectionString);
        }
        else
        {
            // Throwing a descriptive exception is better than using a dummy string that causes transient connection errors.
            throw new InvalidOperationException("Project Multi-tenant Error: Tenant connection string could not be resolved. Ensure the 'X-Tenant-Id' header is present and the tenant exists in the Master Database.");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        // Additional tenant-specific entity configurations go here
    }
}
