namespace SaaS.Application.Interfaces;

public interface ITokenVersionCacheService
{
    Task<int?> GetTokenVersionAsync(string tenantId, string userId, CancellationToken cancellationToken = default);
    Task SetTokenVersionAsync(string tenantId, string userId, int tokenVersion, CancellationToken cancellationToken = default);
}
