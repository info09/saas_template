namespace SaaS.Domain.Entities;

public class Tenant
{
    public string Id { get; set; } = string.Empty; // Unique identifier for the tenant, e.g., "tenant1"
    public string Name { get; set; } = string.Empty; // Display name
    public string ConnectionString { get; set; } = string.Empty; // Database connection string for this tenant
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
