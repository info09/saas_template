using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
        services.AddDbContext<MasterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("MasterConnection")));

        services.AddHttpContextAccessor();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddScoped<ITenantService, TenantService>();

        services.AddDbContext<TenantDbContext>((serviceProvider, options) =>
        {
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var tenantConnectionString = tenantService.GetConnectionString();

            if (string.IsNullOrWhiteSpace(tenantConnectionString))
            {
                var isEfDesignTime = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(assembly => string.Equals(assembly.GetName().Name, "Microsoft.EntityFrameworkCore.Design", StringComparison.Ordinal));

                if (isEfDesignTime)
                {
                    var catalogConnectionString = configuration.GetConnectionString("MasterConnection")
                        ?? throw new InvalidOperationException("Missing catalog connection string.");

                    var builder = new NpgsqlConnectionStringBuilder(catalogConnectionString)
                    {
                        Database = "SaaS_Tenant_Template"
                    };

                    tenantConnectionString = builder.ConnectionString;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Tenant connection string could not be resolved. Ensure the 'X-Tenant-Id' header is present and the tenant exists in the Master Database.");
                }
            }

            options.UseNpgsql(tenantConnectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure());
        });

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

        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<DatabaseMigrationService>();

        return services;
    }
}
