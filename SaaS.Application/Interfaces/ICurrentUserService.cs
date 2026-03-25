namespace SaaS.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? TenantId { get; }
    int? TokenVersion { get; }
    bool IsAuthenticated { get; }
}
