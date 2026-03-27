using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<Result<string>>
{
    public string Name { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
}
