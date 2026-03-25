using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.LogoutAll;

public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, Result>
{
    private readonly IIdentityService _identityService;

    public LogoutAllCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RevokeAllSessionsAsync(request.UserId);
    }
}
