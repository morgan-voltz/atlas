using Atlas.Domain.Notifications;
using FluentValidation;

namespace Atlas.Application.Notifications.RegisterDevice;

/// <summary>
/// Validation des entrées d'enregistrement device (audit Lot 4 — F1). Borne explicitement les longueurs
/// pour répondre par un 400 (ValidationError) plutôt que de laisser <see cref="DeviceRegistration.Register"/>
/// lever une <see cref="System.ArgumentException"/> (qui produirait un 500) sur un token surdimensionné.
/// </summary>
internal sealed class RegisterDeviceValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceValidator()
    {
        RuleFor(command => command.Platform)
            .NotEmpty().WithMessage("La plateforme est requise.");

        RuleFor(command => command.Token)
            .NotEmpty().WithMessage("Le token push est requis.")
            .MaximumLength(DeviceRegistration.MaxTokenLength)
                .WithMessage($"Le token push dépasse {DeviceRegistration.MaxTokenLength} caractères.");

        RuleFor(command => command.Label)
            .MaximumLength(DeviceRegistration.MaxLabelLength)
                .WithMessage($"L'étiquette dépasse {DeviceRegistration.MaxLabelLength} caractères.")
            .When(command => command.Label is not null);
    }
}
