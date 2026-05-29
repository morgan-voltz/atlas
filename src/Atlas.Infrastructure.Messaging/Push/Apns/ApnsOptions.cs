namespace Atlas.Infrastructure.Messaging.Push.Apns;

/// <summary>
/// Configuration de l'adapter Apple Push Notification service (F-020, iOS + macOS).
/// Si l'un des champs requis est vide, la DI saute l'enregistrement APNs ; FCM peut continuer
/// à fonctionner seul. <c>UseSandbox</c> = <c>true</c> pour TestFlight / dev (URL .sandbox.).
/// </summary>
internal sealed class ApnsOptions
{
    public const string SectionName = "Apns";

    /// <summary>Identifiant Apple Developer Team (10 caractères).</summary>
    public string? TeamId { get; set; }

    /// <summary>Identifiant de la clé Auth Key .p8 (10 caractères, visible dans la console developer).</summary>
    public string? KeyId { get; set; }

    /// <summary>
    /// Contenu PEM de la clé privée .p8 (P-256). **Ne jamais committer** — passer via secrets / KMS.
    /// </summary>
    public string? PrivateKeyPem { get; set; }

    /// <summary>Bundle ID de l'app, renseigné dans le header <c>apns-topic</c>.</summary>
    public string? BundleId { get; set; }

    /// <summary>Utilise <c>api.sandbox.push.apple.com</c> au lieu de l'URL prod si vrai.</summary>
    public bool UseSandbox { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
