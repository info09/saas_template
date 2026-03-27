namespace SaaS.Domain.Exceptions;

public class TenantNotFoundException : Exception
{
    public TenantNotFoundException(string id) 
        : base($"Tenant '{id}' was not found.")
    {
    }
}
