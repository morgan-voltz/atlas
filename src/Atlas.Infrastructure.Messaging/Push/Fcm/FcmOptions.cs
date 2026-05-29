namespace Atlas.Infrastructure.Messaging.Push.Fcm;

/// <summary>
/// Configuration de l'adapter Firebase Cloud Messaging (F-020, Android + Web Push).
/// Si <see cref="ServiceAccountJson"/> ou <see cref="ProjectId"/> est vide,
/// la DI revient à <c>LoggingNotificationDispatcher</c> (mode dev / sans push).
/// </summary>
internal sealed class FcmOptions
{
    public const string SectionName = "Fcm";

    /// <summary>ID du projet Firebase (ex. <c>atlas-prod</c>).</summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Contenu JSON brut du service account Firebase (téléchargé depuis la console).
    /// Contient <c>client_email</c> + <c>private_key</c> (PEM) + <c>token_uri</c>.
    /// **Ne jamais committer ce JSON** — passer via secrets / KMS.
    /// </summary>
    public string? ServiceAccountJson { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
