namespace SaaS.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string email, string tenantId, int tokenVersion, Guid sessionId);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiresAtUtc();
    DateTime GetRefreshTokenExpiresAtUtc();
}
