using FluentValidation;

namespace SaaS.Application.Features.Auth.LogoutSession;

public class LogoutSessionCommandValidator : AbstractValidator<LogoutSessionCommand>
{
    public LogoutSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();
    }
}
