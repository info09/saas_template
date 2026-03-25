using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Identity;

namespace SaaS.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(Result Result, AuthUserInfo? User)> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return (Result.Failure("Invalid credentials."), null);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            return (Result.Failure("Invalid credentials."), null);
        }

        return (Result.Success(), new AuthUserInfo(user.Id, user.Email ?? email, user.TokenVersion));
    }

    public async Task<(Result Result, AuthUserInfo? User)> GetByRefreshTokenAsync(string refreshToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(user =>
                user.RefreshToken == refreshToken &&
                user.RefreshTokenExpiresAtUtc.HasValue &&
                user.RefreshTokenExpiresAtUtc.Value > DateTime.UtcNow);

        if (user == null)
        {
            return (Result.Failure("Invalid or expired refresh token."), null);
        }

        return (Result.Success(), new AuthUserInfo(user.Id, user.Email ?? user.UserName ?? string.Empty, user.TokenVersion));
    }

    public async Task<Result> SetRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAtUtc)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAtUtc = expiresAtUtc;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure(updateResult.Errors.Select(error => error.Description).ToArray());
        }

        return Result.Success();
    }

    public async Task<(Result Result, AuthUserInfo? User)> RevokeRefreshTokenAsync(string refreshToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(user => user.RefreshToken == refreshToken);

        if (user == null)
        {
            return (Result.Failure("Invalid refresh token."), null);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiresAtUtc = null;
        user.TokenVersion += 1;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (Result.Failure(updateResult.Errors.Select(error => error.Description).ToArray()), null);
        }

        return (Result.Success("Logged out successfully."), new AuthUserInfo(user.Id, user.Email ?? user.UserName ?? string.Empty, user.TokenVersion));
    }

    public async Task<int?> GetTokenVersionAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.TokenVersion;
    }

    public async Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        return Result.Success();
    }
}
