using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;

namespace SaaS.Application.Features.Auth.Sessions;

public record GetSessionsQuery(string UserId, Guid? CurrentSessionId) : IRequest<Result<IReadOnlyList<UserSessionResponse>>>;
