using FluentValidation;

namespace Atlas.Application.Users.VerifyEmail;

internal sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Token).NotEmpty();
    }
}
