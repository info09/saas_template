using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Dtos.Auth;

namespace SaaS.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string TenantId) : IRequest<Result<LoginResponse>>;
