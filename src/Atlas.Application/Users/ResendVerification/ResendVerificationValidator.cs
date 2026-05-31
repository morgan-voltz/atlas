using FluentValidation;

namespace Atlas.Application.Users.ResendVerification;

internal sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(254);
    }
}
