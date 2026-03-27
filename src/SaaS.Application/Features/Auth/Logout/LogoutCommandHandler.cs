using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService _identityService;

    public LogoutCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RevokeCurrentSessionAsync(request.UserId, request.SessionId);
    }
}
