using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Infrastructure.Services.TenantProvisioning;

namespace SaaS.Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ITenantConnectionStringFactory _tenantConnectionStringFactory;
    private readonly ITenantProvisioningDbContextFactory _tenantDbContextFactory;
    private readonly ITenantSchemaMigrator _tenantSchemaMigrator;
    private readonly ITenantAdminUserSeeder _tenantAdminUserSeeder;

    public TenantProvisioningService(
        IEncryptionService encryptionService,
        ITenantConnectionStringFactory tenantConnectionStringFactory,
        ITenantProvisioningDbContextFactory tenantDbContextFactory,
        ITenantSchemaMigrator tenantSchemaMigrator,
        ITenantAdminUserSeeder tenantAdminUserSeeder)
    {
        _encryptionService = encryptionService;
        _tenantConnectionStringFactory = tenantConnectionStringFactory;
        _tenantDbContextFactory = tenantDbContextFactory;
        _tenantSchemaMigrator = tenantSchemaMigrator;
        _tenantAdminUserSeeder = tenantAdminUserSeeder;
    }

    public async Task ProvisionTenantAsync(Tenant tenant, string adminEmail, string adminPassword, CancellationToken cancellationToken)
    {
        var rawConnectionString = _tenantConnectionStringFactory.CreateRawConnectionString(tenant.Id);
        tenant.ConnectionString = _encryptionService.Encrypt(rawConnectionString);

        await using var tenantDbContext = _tenantDbContextFactory.CreateDbContext(rawConnectionString);
        await _tenantSchemaMigrator.MigrateAsync(tenantDbContext, cancellationToken);
        await _tenantAdminUserSeeder.SeedAsync(tenantDbContext, adminEmail, adminPassword, cancellationToken);
    }
}
