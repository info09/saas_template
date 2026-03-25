using Microsoft.AspNetCore.Identity;

namespace SaaS.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int TokenVersion { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
