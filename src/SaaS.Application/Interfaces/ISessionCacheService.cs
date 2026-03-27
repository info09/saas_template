namespace SaaS.Application.Interfaces;

public interface ISessionCacheService
{
    Task<SessionCacheLookupResult> GetSessionAsync(string tenantId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> SetSessionAsync(string tenantId, Guid sessionId, SessionCacheEntry session, CancellationToken cancellationToken = default);
    Task<bool> RemoveSessionAsync(string tenantId, Guid sessionId, CancellationToken cancellationToken = default);
}
