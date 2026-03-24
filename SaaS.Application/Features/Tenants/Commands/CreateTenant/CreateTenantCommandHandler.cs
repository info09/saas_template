using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<string>>
{
    private readonly ITenantOnboardingService _tenantOnboardingService;

    public CreateTenantCommandHandler(ITenantOnboardingService tenantOnboardingService)
    {
        _tenantOnboardingService = tenantOnboardingService;
    }

    public async Task<Result<string>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        => await _tenantOnboardingService.CreateTenantAsync(
            request.Name,
            request.AdminEmail,
            request.AdminPassword,
            cancellationToken);
}
