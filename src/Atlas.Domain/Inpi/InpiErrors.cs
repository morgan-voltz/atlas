using Atlas.Domain.Common;

namespace Atlas.Domain.Inpi;

public static class InpiErrors
{
    public static readonly DomainError InvalidCredentials = new InvalidInpiCredentialsError();

    public static readonly DomainError ApiAccessNotAllowed = new InpiApiAccessNotAllowedError();

    public static readonly DomainError Unavailable = new InpiUnavailableError();

    public static readonly DomainError NotConnected = new InpiNotConnectedError();

    private sealed record InvalidInpiCredentialsError()
        : DomainError("inpi.invalid_credentials", "Les identifiants INPI fournis sont invalides.");

    private sealed record InpiApiAccessNotAllowedError()
        : DomainError(
            "inpi.api_access_not_allowed",
            "Ce compte INPI n'est pas habilité à l'accès API. Demandez l'activation de l'accès aux API "
            + "« Entreprises / RNE » depuis votre espace data.inpi.fr (rubrique « Mes accès API / SFTP »).");

    private sealed record InpiUnavailableError()
        : DomainError("inpi.unavailable", "Le service INPI est momentanément indisponible. Réessayez plus tard.");

    private sealed record InpiNotConnectedError()
        : DomainError("inpi.not_connected", "Aucun compte INPI connecté.");
}
