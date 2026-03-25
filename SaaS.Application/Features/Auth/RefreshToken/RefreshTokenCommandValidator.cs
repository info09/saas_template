using FluentValidation;

namespace SaaS.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
