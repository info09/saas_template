namespace SaaS.Application.Interfaces;

public enum SessionCacheStatus
{
    Hit,
    Miss,
    Unavailable
}

public record SessionCacheEntry(string UserId, DateTime ExpiresAtUtc, bool IsRevoked);

public record SessionCacheLookupResult(SessionCacheStatus Status, SessionCacheEntry? Session);
