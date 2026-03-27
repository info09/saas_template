using FluentValidation;

namespace SaaS.Application.Features.Auth.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();
    }
}
