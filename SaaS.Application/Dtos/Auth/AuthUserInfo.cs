namespace SaaS.Application.Dtos.Auth;

public record AuthUserInfo(string UserId, string Email, int TokenVersion);
