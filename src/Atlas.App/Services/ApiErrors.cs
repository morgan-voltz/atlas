using Atlas.Shared.Result;

namespace Atlas.App.Services;

/// <summary>Erreurs côté client liées aux appels à l'API Atlas (ADR-002).</summary>
public sealed record ApiError(string Code, string Message) : Error(Code, Message);

public static class ApiErrors
{
    public static ApiError RequestFailed(int statusCode) =>
        new("api.request_failed", $"L'appel à l'API a échoué (HTTP {statusCode}).");

    public static ApiError Unreachable() =>
        new("api.unreachable", "Impossible de joindre le serveur Atlas. Vérifiez votre connexion.");

    public static ApiError InvalidCredentials() =>
        new("api.invalid_credentials", "Adresse e-mail ou mot de passe incorrect.");

    public static ApiError SessionExpired() =>
        new("api.session_expired", "Votre session a expiré. Reconnectez-vous.");

    /// <summary>État dégradé honnête (doc 12 §10) : la donnée RNE exige une connexion INPI.</summary>
    public static ApiError InpiNotConnected() =>
        new("inpi.not_connected", "Connectez votre compte INPI (Profil) pour rechercher des entreprises.");

    /// <summary>Le compte INPI n'est pas habilité à l'API (un compte portail standard ne suffit pas).</summary>
    public static ApiError InpiAccessNotAllowed() =>
        new("inpi.api_access_not_allowed", "Ce compte INPI n'est pas habilité à l'API INPI (un compte portail standard ne suffit pas).");

    public static ApiError InpiConnectionFailed() =>
        new("inpi.connection_failed", "La connexion INPI a échoué. Vérifiez vos identifiants.");
}
