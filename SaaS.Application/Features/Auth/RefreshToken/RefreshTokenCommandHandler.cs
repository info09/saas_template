using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ITokenVersionCacheService _tokenVersionCacheService;

    public RefreshTokenCommandHandler(IIdentityService identityService, ITokenService tokenService, ITokenVersionCacheService tokenVersionCacheService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _tokenVersionCacheService = tokenVersionCacheService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.GetByRefreshTokenAsync(request.RefreshToken);
        if (!result.Succeeded || user is null)
        {
            return Result<LoginResponse>.Failure(result.Errors ?? ["Invalid or expired refresh token."]);
        }

        if (user.SessionId is null)
        {
            return Result<LoginResponse>.Failure("The refresh token is not associated with an active session.");
        }

        var accessTokenExpiresAtUtc = _tokenService.GetAccessTokenExpiresAtUtc();
        var refreshTokenExpiresAtUtc = _tokenService.GetRefreshTokenExpiresAtUtc();
        var refreshToken = _tokenService.GenerateRefreshToken();

        var saveRefreshTokenResult = await _identityService.RotateRefreshTokenAsync(
            request.RefreshToken,
            refreshToken,
            refreshTokenExpiresAtUtc);

        if (!saveRefreshTokenResult.Succeeded)
        {
            return Result<LoginResponse>.Failure(saveRefreshTokenResult.Errors ?? ["Unable to persist refresh token."]);
        }

        var accessToken = _tokenService.GenerateJwtToken(
            user.UserId,
            user.Email,
            request.TenantId,
            user.TokenVersion,
            user.SessionId.Value);

        _ = await _tokenVersionCacheService.SetTokenVersionAsync(
            request.TenantId,
            user.UserId,
            user.TokenVersion,
            cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshToken,
            accessTokenExpiresAtUtc,
            refreshTokenExpiresAtUtc));
    }
}
