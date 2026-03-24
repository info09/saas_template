using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Login
{
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
            var (result, userId) = await _identityService.AuthenticateAsync(request.Email, request.Password, request.TenantId);

            if (!result.Succeeded)
            {
                return Result<LoginResponse>.Failure(result.Errors!);
            }

            var token = _tokenService.GenerateJwtToken(userId, request.Email, request.TenantId);

            return Result<LoginResponse>.Success(new LoginResponse(true, token, null));
        }
    }
}
