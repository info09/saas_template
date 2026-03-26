using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Identity;
using SaaS.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace SaaS.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TenantDbContext _tenantDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantService _tenantService;
    private readonly ISessionCacheService _sessionCacheService;

    public IdentityService(
        UserManager<AppUser> userManager,
        TenantDbContext tenantDbContext,
        IHttpContextAccessor httpContextAccessor,
        ITenantService tenantService,
        ISessionCacheService sessionCacheService)
    {
        _userManager = userManager;
        _tenantDbContext = tenantDbContext;
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
        _sessionCacheService = sessionCacheService;
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

        return (Result.Success(), new AuthUserInfo(user.Id, user.Email ?? email));
    }

    public async Task<(Result Result, AuthUserInfo? User)> GetByRefreshTokenAsync(string refreshToken)
    {
        var refreshTokenHash = HashRefreshToken(refreshToken);
        var session = await _tenantDbContext.UserSessions
            .Include(userSession => userSession.User)
            .FirstOrDefaultAsync(userSession =>
                userSession.RefreshTokenHash == refreshTokenHash &&
                userSession.RevokedAtUtc == null &&
                userSession.ExpiresAtUtc > DateTime.UtcNow);

        if (session?.User == null)
        {
            return (Result.Failure("Invalid or expired refresh token."), null);
        }

        session.LastSeenAtUtc = DateTime.UtcNow;
        session.LastSeenIp = GetRemoteIpAddress();
        session.UserAgent = GetUserAgent();
        await _tenantDbContext.SaveChangesAsync();

        var user = session.User;
        await CacheSessionAsync(session);
        return (Result.Success(), new AuthUserInfo(user.Id, user.Email ?? user.UserName ?? string.Empty, session.Id));
    }

    public async Task<UserProfileResponse?> GetProfileAsync(string userId, string tenantId)
    {
        var user = await _userManager.Users
            .Where(user => user.Id == userId)
            .Select(user => new UserProfileResponse(
                user.Id,
                user.Email ?? user.UserName ?? string.Empty,
                user.FirstName,
                user.LastName,
                tenantId))
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<IReadOnlyList<UserSessionResponse>> GetSessionsAsync(string userId, Guid? currentSessionId)
    {
        var now = DateTime.UtcNow;

        return await _tenantDbContext.UserSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.CreatedAtUtc)
            .Select(session => new UserSessionResponse(
                session.Id,
                session.CreatedAtUtc,
                session.ExpiresAtUtc,
                session.LastSeenAtUtc,
                session.RevokedAtUtc,
                session.CreatedByIp,
                session.LastSeenIp,
                session.UserAgent,
                session.DeviceName,
                currentSessionId.HasValue && session.Id == currentSessionId.Value,
                session.RevokedAtUtc == null && session.ExpiresAtUtc > now))
            .ToListAsync();
    }

    public async Task<(Result Result, Guid? SessionId)> CreateSessionAsync(string userId, string refreshToken, DateTime expiresAtUtc)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (Result.Failure("User not found."), null);
        }

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAtUtc,
            LastSeenAtUtc = DateTime.UtcNow,
            CreatedByIp = GetRemoteIpAddress(),
            LastSeenIp = GetRemoteIpAddress(),
            UserAgent = GetUserAgent()
        };

        await _tenantDbContext.UserSessions.AddAsync(session);
        await _tenantDbContext.SaveChangesAsync();
        await CacheSessionAsync(session);

        return (Result.Success(), session.Id);
    }

    public async Task<Result> RotateRefreshTokenAsync(string currentRefreshToken, string newRefreshToken, DateTime expiresAtUtc)
    {
        var currentRefreshTokenHash = HashRefreshToken(currentRefreshToken);
        var session = await _tenantDbContext.UserSessions
            .FirstOrDefaultAsync(userSession =>
                userSession.RefreshTokenHash == currentRefreshTokenHash &&
                userSession.RevokedAtUtc == null &&
                userSession.ExpiresAtUtc > DateTime.UtcNow);

        if (session == null)
        {
            return Result.Failure("Invalid or expired refresh token.");
        }

        session.RefreshTokenHash = HashRefreshToken(newRefreshToken);
        session.ExpiresAtUtc = expiresAtUtc;
        session.LastSeenAtUtc = DateTime.UtcNow;
        session.LastSeenIp = GetRemoteIpAddress();
        session.UserAgent = GetUserAgent();

        await _tenantDbContext.SaveChangesAsync();
        await CacheSessionAsync(session);

        return Result.Success();
    }

    public async Task<(Result Result, AuthUserInfo? User)> RevokeRefreshTokenAsync(string userId, Guid sessionId)
    {
        //var refreshTokenHash = HashRefreshToken(refreshToken);
        var session = await _tenantDbContext.UserSessions
            .Include(userSession => userSession.User)
            .FirstOrDefaultAsync(userSession =>
                userSession.Id == sessionId &&
                userSession.UserId == userId &&
                userSession.RevokedAtUtc == null);

        if (session?.User == null)
        {
            return (Result.Failure("Invalid refresh token."), null);
        }

        var user = session.User;
        session.RevokedAtUtc = DateTime.UtcNow;
        session.LastSeenAtUtc = DateTime.UtcNow;
        session.LastSeenIp = GetRemoteIpAddress();
        session.UserAgent = GetUserAgent();

        await _tenantDbContext.SaveChangesAsync();
        await CacheSessionAsync(session);

        return (Result.Success("Logged out successfully."), new AuthUserInfo(user.Id, user.Email ?? user.UserName ?? string.Empty));
    }

    public async Task<Result> RevokeSessionAsync(string userId, Guid sessionId)
    {
        var session = await _tenantDbContext.UserSessions
            .FirstOrDefaultAsync(userSession => userSession.Id == sessionId && userSession.UserId == userId);

        if (session == null)
        {
            return Result.Failure("Session not found.");
        }

        if (session.RevokedAtUtc.HasValue)
        {
            return Result.Success("Session already revoked.");
        }

        session.RevokedAtUtc = DateTime.UtcNow;
        session.LastSeenAtUtc = DateTime.UtcNow;
        session.LastSeenIp = GetRemoteIpAddress();
        session.UserAgent = GetUserAgent();

        await _tenantDbContext.SaveChangesAsync();
        await CacheSessionAsync(session);

        return Result.Success("Session revoked successfully.");
    }

    public async Task<Result> RevokeAllSessionsAsync(string userId)
    {
        var sessions = await _tenantDbContext.UserSessions
            .Where(userSession => userSession.UserId == userId && userSession.RevokedAtUtc == null)
            .ToListAsync();

        if (sessions.Count == 0)
        {
            return Result.Success("No active sessions found.");
        }

        var now = DateTime.UtcNow;
        var remoteIpAddress = GetRemoteIpAddress();
        var userAgent = GetUserAgent();

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = now;
            session.LastSeenAtUtc = now;
            session.LastSeenIp = remoteIpAddress;
            session.UserAgent = userAgent;
        }

        await _tenantDbContext.SaveChangesAsync();

        foreach (var session in sessions)
        {
            await CacheSessionAsync(session);
        }

        return Result.Success("All sessions revoked successfully.");
    }

    public async Task<SessionCacheEntry?> GetSessionStateAsync(string userId, Guid sessionId)
    {
        var session = await _tenantDbContext.UserSessions
            .AsNoTracking()
            .Where(userSession => userSession.Id == sessionId && userSession.UserId == userId)
            .Select(userSession => new SessionCacheEntry(
                userSession.UserId,
                userSession.ExpiresAtUtc,
                userSession.RevokedAtUtc != null))
            .FirstOrDefaultAsync();

        return session;
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

    private string? GetRemoteIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    private async Task CacheSessionAsync(UserSession session)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }

        _ = await _sessionCacheService.SetSessionAsync(
            tenantId,
            session.Id,
            new SessionCacheEntry(session.UserId, session.ExpiresAtUtc, session.RevokedAtUtc != null));
    }
}
