using FluentValidation;

namespace Atlas.Application.Users.TwoFactor;

internal sealed class EnableTwoFactorValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty();
    }
}

internal sealed class VerifyTwoFactorValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorValidator()
    {
        RuleFor(command => command.ChallengeToken).NotEmpty();
        RuleFor(command => command.Code).NotEmpty();
    }
}

internal sealed class DisableTwoFactorValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty();
    }
}
