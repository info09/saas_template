using FluentValidation;

namespace SaaS.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .MinimumLength(6);
    }
}
