using FluentValidation;

namespace Atlas.Application.Users.Register;

internal sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    // Politique ANSSI : la longueur prime sur la complexité (cf. docs/04-securite-rgpd.md §5.4.1).
    private const int MinPasswordLength = 12;
    private const int MaxPasswordLength = 256;

    public RegisterUserValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(254);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(MinPasswordLength)
            .WithMessage($"Le mot de passe doit contenir au moins {MinPasswordLength} caractères.")
            .MaximumLength(MaxPasswordLength);
    }
}
