using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Identity;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly CatalogDbContext _catalogDb;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEncryptionService _encryptionService;

    public TenantProvisioningService(CatalogDbContext catalogDb, IServiceProvider serviceProvider, IEncryptionService encryptionService)
    {
        _catalogDb = catalogDb;
        _serviceProvider = serviceProvider;
        _encryptionService = encryptionService;
    }

    public async Task ProvisionTenantAsync(Tenant tenant, string adminEmail, string adminPassword, CancellationToken cancellationToken)
    {
        var catalogConnString = _catalogDb.Database.GetDbConnection().ConnectionString;
        var builder = new NpgsqlConnectionStringBuilder(catalogConnString);
        
        var dbName = $"SaaS_Tenant_{tenant.Id}";
        builder.Database = dbName;

        var rawConnectionString = builder.ToString();
        tenant.ConnectionString = _encryptionService.Encrypt(rawConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(rawConnectionString);

        var dummyTenantService = new DummyProvisioningTenantService(tenant.Id, tenant.ConnectionString);

        using var context = new TenantDbContext(optionsBuilder.Options, dummyTenantService);

        await context.Database.MigrateAsync(cancellationToken);

        await SeedAdminUserAsync(context, adminEmail, adminPassword, cancellationToken);
    }
    
    private async Task SeedAdminUserAsync(TenantDbContext context, string email, string password, CancellationToken ct)
    {
        var hasher = new PasswordHasher<AppUser>();
        var adminRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN" };
        context.Roles.Add(adminRole);

        var adminUser = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            ConcurrencyStamp = Guid.NewGuid().ToString("D")
        };
        
        adminUser.PasswordHash = hasher.HashPassword(adminUser, password);

        context.Users.Add(adminUser);
        context.UserRoles.Add(new IdentityUserRole<string> { RoleId = adminRole.Id, UserId = adminUser.Id });

        await context.SaveChangesAsync(ct);
    }
}

public class DummyProvisioningTenantService : ITenantService
{
    private readonly string _tenantId;
    private readonly string _connectionString;
    
    public DummyProvisioningTenantService(string tenantId, string connectionString)
    {
        _tenantId = tenantId;
        _connectionString = connectionString;
    }
    
    public string? GetCurrentTenantId() => _tenantId;
    public string? GetConnectionString() => _connectionString;
}
