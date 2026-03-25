using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenVersionCacheService _tokenVersionCacheService;

    public LogoutCommandHandler(IIdentityService identityService, ITokenVersionCacheService tokenVersionCacheService)
    {
        _identityService = identityService;
        _tokenVersionCacheService = tokenVersionCacheService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.RevokeRefreshTokenAsync(request.RefreshToken);
        if (!result.Succeeded || user is null)
        {
            return result;
        }

        _ = await _tokenVersionCacheService.SetTokenVersionAsync(
            request.TenantId,
            user.UserId,
            user.TokenVersion,
            cancellationToken);

        return result;
    }
}
