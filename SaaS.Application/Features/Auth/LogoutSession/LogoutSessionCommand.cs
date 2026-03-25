using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.LogoutSession;

public record LogoutSessionCommand(string UserId, Guid SessionId) : IRequest<Result>;
