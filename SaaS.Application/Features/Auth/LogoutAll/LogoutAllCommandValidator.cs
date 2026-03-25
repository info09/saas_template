using FluentValidation;

namespace SaaS.Application.Features.Auth.LogoutAll;

public class LogoutAllCommandValidator : AbstractValidator<LogoutAllCommand>
{
    public LogoutAllCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
