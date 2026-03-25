using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;

namespace SaaS.Application.Features.Auth.Profile;

public record GetProfileQuery(string UserId, string TenantId) : IRequest<Result<UserProfileResponse>>;
