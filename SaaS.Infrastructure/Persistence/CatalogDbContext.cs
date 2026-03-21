using Microsoft.EntityFrameworkCore;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Persistence;

/// <summary>
/// Master database context containing all tenants.
/// </summary>
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tenant configuration
        modelBuilder.Entity<Tenant>().HasKey(t => t.Id);
        modelBuilder.Entity<Tenant>().Property(t => t.Id).HasMaxLength(50);
        modelBuilder.Entity<Tenant>().Property(t => t.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Tenant>().Property(t => t.ConnectionString).IsRequired();
    }
}
