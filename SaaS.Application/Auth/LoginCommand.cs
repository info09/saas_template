using MediatR;
using SaaS.Application.Common;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Auth;

public record LoginCommand(string Email, string Password, string TenantId) : IRequest<LoginResponse>;

public record LoginResponse(bool Succeeded, string Token, string[] Errors);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.AuthenticateAsync(request.Email, request.Password, request.TenantId);

        if (!result.Succeeded)
        {
            return new LoginResponse(false, string.Empty, result.Errors);
        }

        var token = _tokenService.GenerateJwtToken(userId, request.Email, request.TenantId);

        return new LoginResponse(true, token, Array.Empty<string>());
    }
}
