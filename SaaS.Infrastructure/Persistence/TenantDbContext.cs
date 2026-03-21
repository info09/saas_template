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
        var tenantConnectionString = _tenantService.GetConnectionString();
        
        if (!string.IsNullOrEmpty(tenantConnectionString))
        {
            // Dynamically set connection string based on current tenant
            optionsBuilder.UseNpgsql(tenantConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        // Additional tenant-specific entity configurations go here
    }
}
