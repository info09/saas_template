using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Identity;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly MasterDbContext _masterDb;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;

    public TenantProvisioningService(MasterDbContext masterDb, IEncryptionService encryptionService, IConfiguration configuration)
    {
        _masterDb = masterDb;
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public async Task ProvisionTenantAsync(Tenant tenant, string adminEmail, string adminPassword, CancellationToken cancellationToken)
    {
        //var conn = _masterDb.Database.GetDbConnection();
        //var builder = new NpgsqlConnectionStringBuilder(conn.ConnectionString);

        var masterConnectionString = _configuration.GetConnectionString("MasterConnection") ?? throw new InvalidOperationException("Missing catalog connection string.");

        var builder = new NpgsqlConnectionStringBuilder(masterConnectionString)
        {
            Database = $"SaaS_Tenant_{tenant.Id}"
        };

        var rawConnectionString = builder.ToString();
        tenant.ConnectionString = _encryptionService.Encrypt(rawConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(rawConnectionString);

        using var context = new TenantDbContext(optionsBuilder.Options);

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
            FirstName = "admin",
            LastName = "admin",
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
