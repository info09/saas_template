using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Profile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileResponse>>
{
    private readonly IIdentityService _identityService;

    public GetProfileQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<UserProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _identityService.GetProfileAsync(request.UserId, request.TenantId);
        if (profile is null)
        {
            return Result<UserProfileResponse>.Failure("User profile not found.");
        }

        return Result<UserProfileResponse>.Success(profile);
    }
}
