using Microsoft.Extensions.Caching.Distributed;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Services;

public class TokenVersionCacheService : ITokenVersionCacheService
{
    private readonly IDistributedCache _distributedCache;

    public TokenVersionCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<int?> GetTokenVersionAsync(string tenantId, string userId, CancellationToken cancellationToken = default)
    {
        var value = await _distributedCache.GetStringAsync(BuildKey(tenantId, userId), cancellationToken);
        return int.TryParse(value, out var tokenVersion) ? tokenVersion : null;
    }

    public async Task SetTokenVersionAsync(string tenantId, string userId, int tokenVersion, CancellationToken cancellationToken = default)
    {
        await _distributedCache.SetStringAsync(
            BuildKey(tenantId, userId),
            tokenVersion.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },
            cancellationToken);
    }

    private static string BuildKey(string tenantId, string userId) => $"auth:token-version:{tenantId}:{userId}";
}
