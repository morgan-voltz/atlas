using Atlas.Domain.Common;

namespace Atlas.Domain.Inpi;

public static class InpiErrors
{
    public static readonly DomainError InvalidCredentials = new InvalidInpiCredentialsError();

    public static readonly DomainError Unavailable = new InpiUnavailableError();

    public static readonly DomainError NotConnected = new InpiNotConnectedError();

    private sealed record InvalidInpiCredentialsError()
        : DomainError("inpi.invalid_credentials", "Les identifiants INPI fournis sont invalides.");

    private sealed record InpiUnavailableError()
        : DomainError("inpi.unavailable", "Le service INPI est momentanément indisponible. Réessayez plus tard.");

    private sealed record InpiNotConnectedError()
        : DomainError("inpi.not_connected", "Aucun compte INPI connecté.");
}
