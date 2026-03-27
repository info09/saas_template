using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaS.Infrastructure.Identity;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public class TenantAdminUserSeeder : ITenantAdminUserSeeder
{
    public async Task SeedAsync(TenantDbContext tenantDbContext, string adminEmail, string adminPassword, CancellationToken cancellationToken)
    {
        const string adminRoleName = "Admin";
        const string normalizedAdminRoleName = "ADMIN";

        var adminRole = await tenantDbContext.Roles
            .FirstOrDefaultAsync(role => role.NormalizedName == normalizedAdminRoleName, cancellationToken);

        if (adminRole is null)
        {
            adminRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = adminRoleName,
                NormalizedName = normalizedAdminRoleName
            };

            tenantDbContext.Roles.Add(adminRole);
        }

        var normalizedEmail = adminEmail.ToUpperInvariant();
        var adminUser = await tenantDbContext.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "admin",
                LastName = "admin",
                UserName = adminEmail,
                NormalizedUserName = normalizedEmail,
                Email = adminEmail,
                NormalizedEmail = normalizedEmail,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };

            var hasher = new PasswordHasher<AppUser>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);
            tenantDbContext.Users.Add(adminUser);
        }

        var userHasAdminRole = await tenantDbContext.UserRoles
            .AnyAsync(userRole => userRole.RoleId == adminRole.Id && userRole.UserId == adminUser.Id, cancellationToken);

        if (!userHasAdminRole)
        {
            tenantDbContext.UserRoles.Add(new IdentityUserRole<string>
            {
                RoleId = adminRole.Id,
                UserId = adminUser.Id
            });
        }

        await tenantDbContext.SaveChangesAsync(cancellationToken);
    }
}
