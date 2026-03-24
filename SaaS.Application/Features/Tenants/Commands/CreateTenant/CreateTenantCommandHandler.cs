using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<string>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantProvisioningService _provisioningService;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository, ITenantProvisioningService provisioningService)
    {
        _tenantRepository = tenantRepository;
        _provisioningService = provisioningService;
    }

    public async Task<Result<string>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var isExites = await _tenantRepository.Queryable().AnyAsync(i => i.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (isExites)
        {
            return Result<string>.Failure("Tenant is already exists with the same name. Please choose a different name.");
        }
        var tenantId = Guid.NewGuid().ToString();

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = request.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _provisioningService.ProvisionTenantAsync(tenant, request.AdminEmail, request.AdminPassword, cancellationToken);
        await _tenantRepository.CreateTenantAsync(tenant, cancellationToken);
        return Result<string>.Success(tenant.Id);
    }
}
