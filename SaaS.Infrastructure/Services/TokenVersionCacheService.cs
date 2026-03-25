using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Services;

public class TokenVersionCacheService : ITokenVersionCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<TokenVersionCacheService> _logger;

    public TokenVersionCacheService(IDistributedCache distributedCache, ILogger<TokenVersionCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<TokenVersionCacheLookupResult> GetTokenVersionAsync(string tenantId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _distributedCache.GetStringAsync(BuildKey(tenantId, userId), cancellationToken);
            if (string.IsNullOrWhiteSpace(value))
            {
                return new TokenVersionCacheLookupResult(TokenVersionCacheStatus.Miss, null);
            }

            return int.TryParse(value, out var tokenVersion)
                ? new TokenVersionCacheLookupResult(TokenVersionCacheStatus.Hit, tokenVersion)
                : new TokenVersionCacheLookupResult(TokenVersionCacheStatus.Miss, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Token version cache lookup failed for tenant {TenantId}, user {UserId}. Falling back to database validation.",
                tenantId,
                userId);

            return new TokenVersionCacheLookupResult(TokenVersionCacheStatus.Unavailable, null);
        }
    }

    public async Task<bool> SetTokenVersionAsync(string tenantId, string userId, int tokenVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.SetStringAsync(
                BuildKey(tenantId, userId),
                tokenVersion.ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                },
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Token version cache write failed for tenant {TenantId}, user {UserId}, tokenVersion {TokenVersion}.",
                tenantId,
                userId,
                tokenVersion);

            return false;
        }
    }

    private static string BuildKey(string tenantId, string userId) => $"auth:token-version:{tenantId}:{userId}";
}
