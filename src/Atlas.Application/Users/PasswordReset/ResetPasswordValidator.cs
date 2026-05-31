using FluentValidation;

namespace Atlas.Application.Users.PasswordReset;

internal sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    // Même politique que l'inscription : la longueur prime (cf. docs/04-securite-rgpd.md §5.4.1).
    private const int MinPasswordLength = 12;
    private const int MaxPasswordLength = 256;

    public ResetPasswordValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Token)
            .NotEmpty();

        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .MinimumLength(MinPasswordLength)
            .WithMessage($"Le mot de passe doit contenir au moins {MinPasswordLength} caractères.")
            .MaximumLength(MaxPasswordLength);
    }
}
