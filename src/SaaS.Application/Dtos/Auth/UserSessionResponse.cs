namespace SaaS.Application.Dtos.Auth;

public record UserSessionResponse(
    Guid SessionId,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? LastSeenAtUtc,
    DateTime? RevokedAtUtc,
    string? CreatedByIp,
    string? LastSeenIp,
    string? UserAgent,
    string? DeviceName,
    bool IsCurrentSession,
    bool IsActive);
