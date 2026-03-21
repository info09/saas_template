using FluentValidation;
using MediatR;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<string>
{
    public string Name { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, string>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantProvisioningService _provisioningService;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository, ITenantProvisioningService provisioningService)
    {
        _tenantRepository = tenantRepository;
        _provisioningService = provisioningService;
    }

    public async Task<string> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = Guid.NewGuid().ToString("N").Substring(0, 10);
        
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = request.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _provisioningService.ProvisionTenantAsync(tenant, request.AdminEmail, request.AdminPassword, cancellationToken);
        await _tenantRepository.CreateTenantAsync(tenant, cancellationToken);

        return tenant.Id;
    }
}

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.AdminEmail)
            .EmailAddress()
            .NotEmpty();

        RuleFor(v => v.AdminPassword)
            .MinimumLength(6)
            .NotEmpty();
    }
}
