using MediatR;
using SaaS.Application.Common;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Auth;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName, string TenantId) : IRequest<Result>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.CreateUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
    }
}
