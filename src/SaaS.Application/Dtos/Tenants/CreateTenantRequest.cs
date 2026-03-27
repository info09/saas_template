namespace SaaS.Application.Dtos.Tenants;

public record CreateTenantRequest(string Name, string AdminEmail, string AdminPassword);
