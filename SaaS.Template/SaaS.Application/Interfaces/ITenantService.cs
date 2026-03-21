namespace SaaS.Application.Interfaces;

public interface ITenantService
{
    /// <summary>
    /// Gets the current Tenant ID from the request (e.g., HTTP Header X-Tenant-Id)
    /// </summary>
    string? GetCurrentTenantId();

    /// <summary>
    /// Gets the database connection string for the current Tenant
    /// </summary>
    string? GetConnectionString();
}
