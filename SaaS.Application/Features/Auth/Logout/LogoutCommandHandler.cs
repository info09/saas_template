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
        var (result, user) = await _identityService.RevokeRefreshTokenAsync(request.UserId, request.SessionId);
        if (!result.Succeeded || user is null)
        {
            return result;
        }

        return result;
    }
}
