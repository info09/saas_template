using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Logout;

public record LogoutCommand(string UserId, Guid SessionId, string TenantId) : IRequest<Result>;
