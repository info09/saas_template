using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaS.Infrastructure.Identity;

namespace SaaS.Infrastructure.Persistence;

/// <summary>
/// Tenant-specific database context.
/// </summary>
public class TenantDbContext : IdentityDbContext<AppUser>
{
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        // Additional tenant-specific entity configurations go here
    }
}
