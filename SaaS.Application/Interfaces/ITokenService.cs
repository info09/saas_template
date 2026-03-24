namespace SaaS.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string email, string tenantId);
}
