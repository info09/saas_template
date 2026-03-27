using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SaaS.Infrastructure.Services.TenantProvisioning;

public class TenantConnectionStringFactory : ITenantConnectionStringFactory
{
    private readonly IConfiguration _configuration;

    public TenantConnectionStringFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateRawConnectionString(string tenantId)
    {
        var masterConnectionString = _configuration.GetConnectionString("MasterConnection")
            ?? throw new InvalidOperationException("Missing master connection string.");

        var builder = new NpgsqlConnectionStringBuilder(masterConnectionString)
        {
            Database = $"SaaS_Tenant_{tenantId}"
        };

        return builder.ConnectionString;
    }
}
