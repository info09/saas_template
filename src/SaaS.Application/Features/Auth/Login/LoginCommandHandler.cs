using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
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
        var refreshToken = _tokenService.GenerateRefreshToken();

        var (saveRefreshTokenResult, sessionId) = await _identityService.CreateSessionAsync(
            user.UserId,
            refreshToken,
            refreshTokenExpiresAtUtc);

        if (!saveRefreshTokenResult.Succeeded || sessionId is null)
        {
            return Result<LoginResponse>.Failure(saveRefreshTokenResult.Errors!);
        }

        var accessToken = _tokenService.GenerateJwtToken(
            user.UserId,
            user.Email,
            request.TenantId,
            sessionId.Value);

        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshToken,
            accessTokenExpiresAtUtc,
            refreshTokenExpiresAtUtc));
    }
}
