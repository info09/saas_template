using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Identity;
using SaaS.Infrastructure.Persistence;
using SaaS.Infrastructure.Repositories;
using SaaS.Infrastructure.Services;

namespace SaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Add Catalog DbContext (Master DB)
        services.AddDbContext<MasterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CatalogConnection")));

        // 2. Add Tenant DbContext (Tenant-specific DB)
        services.AddDbContext<TenantDbContext>(options =>
        {
            // Connection is set dynamically inside the DbContext using ITenantService
        });

        // 3. Add Identity using TenantDbContext
        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TenantDbContext>()
            .AddDefaultTokenProviders();

        // 4. Register Tenant Service
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();



        return services;
    }
}
