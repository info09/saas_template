namespace SaaS.Infrastructure.Services.TenantProvisioning;

public interface ITenantConnectionStringFactory
{
    string CreateRawConnectionString(string tenantId);
}
