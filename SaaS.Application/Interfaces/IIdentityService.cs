using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;

namespace SaaS.Application.Interfaces;

public interface IIdentityService
{
    Task<(Result Result, AuthUserInfo? User)> AuthenticateAsync(string email, string password);
    Task<(Result Result, AuthUserInfo? User)> GetByRefreshTokenAsync(string refreshToken);
    Task<UserProfileResponse?> GetProfileAsync(string userId, string tenantId);
    Task<IReadOnlyList<UserSessionResponse>> GetSessionsAsync(string userId, Guid? currentSessionId);
    Task<(Result Result, Guid? SessionId)> CreateSessionAsync(string userId, string refreshToken, DateTime expiresAtUtc);
    Task<Result> RotateRefreshTokenAsync(string currentRefreshToken, string newRefreshToken, DateTime expiresAtUtc);
    Task<(Result Result, AuthUserInfo? User)> RevokeRefreshTokenAsync(string refreshToken);
    Task<Result> RevokeSessionAsync(string userId, Guid sessionId);
    Task<Result> RevokeAllSessionsAsync(string userId);
    Task<int?> GetTokenVersionAsync(string userId);
    Task<bool?> IsSessionActiveAsync(string userId, Guid sessionId);
    Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName);
}
