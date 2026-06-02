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

    public static ApiError InvalidTwoFactorCode() =>
        new("api.invalid_2fa_code", "Code de validation incorrect. Réessayez avec un code à jour ou un code de secours.");

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

    public static ApiError CompanyNotFound(string siren) =>
        new("companies.not_found", $"Aucune entreprise au RNE pour le SIREN {siren}.");

    /// <summary>Inscription : une adresse déjà utilisée (on invite à se connecter, sans en dire plus).</summary>
    public static ApiError EmailAlreadyInUse() =>
        new("users.email_already_in_use", "Un compte existe déjà avec cette adresse. Connectez-vous.");

    public static ApiError InvalidEmail() =>
        new("users.invalid_email", "Format d'adresse e-mail invalide.");

    public static ApiError RegistrationFailed() =>
        new("users.registration_failed", "La création de compte a échoué. Vérifiez vos informations et réessayez.");

    /// <summary>Lien de vérification d'email invalide ou expiré.</summary>
    public static ApiError InvalidVerificationLink() =>
        new("users.invalid_verification_link", "Ce lien de vérification est invalide ou a expiré.");

    /// <summary>Lien de réinitialisation invalide ou expiré.</summary>
    public static ApiError InvalidPasswordResetToken() =>
        new("users.invalid_password_reset_token", "Ce lien de réinitialisation est invalide ou a expiré. Demandez-en un nouveau.");

    public static ApiError PasswordResetFailed() =>
        new("users.password_reset_failed", "La réinitialisation a échoué. Réessayez.");
}
