namespace SaaS.Infrastructure.Identity;

public class UserSession
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? RefreshTokenHash { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? CreatedByIp { get; set; }
    public string? LastSeenIp { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
    public AppUser User { get; set; } = null!;
}
