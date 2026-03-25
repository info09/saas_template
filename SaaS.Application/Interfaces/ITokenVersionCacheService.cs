namespace SaaS.Application.Interfaces;

public interface ITokenVersionCacheService
{
    Task<TokenVersionCacheLookupResult> GetTokenVersionAsync(string tenantId, string userId, CancellationToken cancellationToken = default);
    Task<bool> SetTokenVersionAsync(string tenantId, string userId, int tokenVersion, CancellationToken cancellationToken = default);
}
