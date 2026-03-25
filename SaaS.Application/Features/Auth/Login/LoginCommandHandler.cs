using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ITokenVersionCacheService _tokenVersionCacheService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService, ITokenVersionCacheService tokenVersionCacheService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _tokenVersionCacheService = tokenVersionCacheService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.AuthenticateAsync(request.Email, request.Password);

        if (!result.Succeeded || user is null)
        {
            return Result<LoginResponse>.Failure(result.Errors!);
        }

        var accessTokenExpiresAtUtc = _tokenService.GetAccessTokenExpiresAtUtc();
        var refreshTokenExpiresAtUtc = _tokenService.GetRefreshTokenExpiresAtUtc();
        var accessToken = _tokenService.GenerateJwtToken(user.UserId, user.Email, request.TenantId, user.TokenVersion);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var saveRefreshTokenResult = await _identityService.SetRefreshTokenAsync(
            user.UserId,
            refreshToken,
            refreshTokenExpiresAtUtc);

        if (!saveRefreshTokenResult.Succeeded)
        {
            return Result<LoginResponse>.Failure(saveRefreshTokenResult.Errors!);
        }

        await _tokenVersionCacheService.SetTokenVersionAsync(
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
