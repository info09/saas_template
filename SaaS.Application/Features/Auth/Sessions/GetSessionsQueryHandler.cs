using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Sessions;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, Result<IReadOnlyList<UserSessionResponse>>>
{
    private readonly IIdentityService _identityService;

    public GetSessionsQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<IReadOnlyList<UserSessionResponse>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _identityService.GetSessionsAsync(request.UserId, request.CurrentSessionId);
        return Result<IReadOnlyList<UserSessionResponse>>.Success(sessions);
    }
}
