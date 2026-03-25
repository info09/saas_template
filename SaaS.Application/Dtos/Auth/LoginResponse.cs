namespace SaaS.Application.Dtos.Auth;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
