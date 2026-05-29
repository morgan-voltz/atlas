namespace Atlas.Infrastructure.Messaging.Email.Brevo;

/// <summary>
/// Configuration de l'adapter Brevo (provider France RGPD-compliant, cf. CLAUDE.md).
/// Si <see cref="ApiKey"/> est vide, la DI revient à <see cref="LoggingEmailSender"/>
/// (mode dev / pas d'envoi réel). Hors <c>Development</c>, <see cref="ApiKey"/> et
/// <see cref="SenderEmail"/> sont requis (<c>ValidateOnStart</c>).
/// </summary>
internal sealed class BrevoOptions
{
    public const string SectionName = "Email:Brevo";

    /// <summary>
    /// Clé d'API du compte Brevo (format <c>xkeysib-...</c>). **Ne jamais committer** —
    /// fournir via secrets / KMS. Sans cette valeur, l'adapter Brevo n'est pas câblé.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>Adresse expéditrice qui apparaît dans les emails. Doit être vérifiée dans Brevo.</summary>
    public string? SenderEmail { get; set; }

    /// <summary>Nom expéditeur affiché à côté de l'adresse.</summary>
    public string SenderName { get; set; } = "Atlas";

    /// <summary>URL de base de l'API Brevo. Surcharge utile en tests d'intégration.</summary>
    public string BaseUrl { get; set; } = "https://api.brevo.com";

    public int TimeoutSeconds { get; set; } = 30;
}
