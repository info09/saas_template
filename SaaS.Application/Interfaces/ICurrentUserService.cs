namespace SaaS.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? TenantId { get; }
    Guid? SessionId { get; }
    bool IsAuthenticated { get; }
}
