using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.LogoutSession;

public class LogoutSessionCommandHandler : IRequestHandler<LogoutSessionCommand, Result>
{
    private readonly IIdentityService _identityService;

    public LogoutSessionCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(LogoutSessionCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RevokeSessionAsync(request.UserId, request.SessionId);
    }
}
