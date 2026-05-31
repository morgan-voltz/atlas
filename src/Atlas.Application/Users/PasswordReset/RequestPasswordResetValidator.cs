using FluentValidation;

namespace Atlas.Application.Users.PasswordReset;

internal sealed class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(254);
    }
}
