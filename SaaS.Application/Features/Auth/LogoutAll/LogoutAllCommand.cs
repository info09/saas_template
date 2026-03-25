using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.LogoutAll;

public record LogoutAllCommand(string UserId) : IRequest<Result>;
