using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;

namespace SaaS.Application.Interfaces;

public interface IIdentityService
{
    Task<(Result Result, AuthUserInfo? User)> AuthenticateAsync(string email, string password);
    Task<(Result Result, AuthUserInfo? User)> GetByRefreshTokenAsync(string refreshToken);
    Task<Result> SetRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAtUtc);
    Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName);
}
