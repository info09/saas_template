using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SaaS.Application.Interfaces;

namespace SaaS.Infrastructure.Services;

public class SessionCacheService : ISessionCacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<SessionCacheService> _logger;

    public SessionCacheService(IDistributedCache distributedCache, ILogger<SessionCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<SessionCacheLookupResult> GetSessionAsync(string tenantId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _distributedCache.GetStringAsync(BuildKey(tenantId, sessionId), cancellationToken);
            if (string.IsNullOrWhiteSpace(value))
            {
                return new SessionCacheLookupResult(SessionCacheStatus.Miss, null);
            }

            var session = JsonSerializer.Deserialize<SessionCacheEntry>(value, SerializerOptions);
            return session is null
                ? new SessionCacheLookupResult(SessionCacheStatus.Miss, null)
                : new SessionCacheLookupResult(SessionCacheStatus.Hit, session);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Session cache lookup failed for tenant {TenantId}, session {SessionId}. Falling back to database validation.",
                tenantId,
                sessionId);

            return new SessionCacheLookupResult(SessionCacheStatus.Unavailable, null);
        }
    }

    public async Task<bool> SetSessionAsync(string tenantId, Guid sessionId, SessionCacheEntry session, CancellationToken cancellationToken = default)
    {
        try
        {
            var ttl = session.ExpiresAtUtc - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                ttl = TimeSpan.FromHours(1);
            }

            await _distributedCache.SetStringAsync(
                BuildKey(tenantId, sessionId),
                JsonSerializer.Serialize(session, SerializerOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Session cache write failed for tenant {TenantId}, session {SessionId}.",
                tenantId,
                sessionId);

            return false;
        }
    }

    public async Task<bool> RemoveSessionAsync(string tenantId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(BuildKey(tenantId, sessionId), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Session cache remove failed for tenant {TenantId}, session {SessionId}.",
                tenantId,
                sessionId);

            return false;
        }
    }

    private static string BuildKey(string tenantId, Guid sessionId) => $"auth:session:{tenantId}:{sessionId}";
}
