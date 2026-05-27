namespace Atlas.Infrastructure.Inpi.Common;

public sealed class InpiOptions
{
    public const string SectionName = "Inpi";

    /// <summary>URL de base de l'API RNE (les chemins sont relatifs, ex. <c>sso/login</c>).</summary>
    public string RneBaseUrl { get; set; } = "https://registre-national-entreprises.inpi.fr/api/";

    /// <summary>URL de base de l'API INPI PI / gateway (marques, brevets, D&amp;M).</summary>
    public string PiBaseUrl { get; set; } = "https://api-gateway.inpi.fr/";

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Durée de vie supposée du token si le JWT RNE n'expose pas d'expiration exploitable.</summary>
    public TimeSpan TokenLifetimeFallback { get; set; } = TimeSpan.FromHours(1);
}
