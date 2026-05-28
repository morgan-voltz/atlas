using Atlas.Domain.Common;

namespace Atlas.Domain.Notifications;

/// <summary>Codes d'erreur métier pour l'enregistrement des devices (F-020).</summary>
public static class DeviceRegistrationErrors
{
    public static DomainError NotFound { get; } = new NotFoundError();

    public static DomainError InvalidToken { get; } = new InvalidTokenError();

    public static DomainError InvalidPlatform(string value) => new InvalidPlatformError(value);

    private sealed record NotFoundError()
        : DomainError("devices.not_found", "Device introuvable.");

    private sealed record InvalidTokenError()
        : DomainError("devices.invalid_token", "Token de notification vide ou invalide.");

    private sealed record InvalidPlatformError(string Value)
        : DomainError("devices.invalid_platform", $"Plateforme inconnue : « {Value} ». Attendu : FcmAndroid, ApnsIos, WindowsWns, MacOsApns.");
}
