using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;

namespace SaaS.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password, string TenantId) : IRequest<Result<LoginResponse>>;
