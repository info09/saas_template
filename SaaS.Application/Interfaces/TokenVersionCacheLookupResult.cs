namespace SaaS.Application.Interfaces;

public enum TokenVersionCacheStatus
{
    Hit,
    Miss,
    Unavailable
}

public record TokenVersionCacheLookupResult(TokenVersionCacheStatus Status, int? TokenVersion);
