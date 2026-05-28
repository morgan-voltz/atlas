namespace Atlas.Infrastructure.Bodacc;

/// <summary>
/// Configuration de l'adapter BODACC (F-048).
/// L'API publique Opendatasoft <c>annonces-commerciales</c> est anonyme ; on rate-limit côté client
/// (un seul polling par jour par SIREN dans le job F-048).
/// </summary>
internal sealed class BodaccOptions
{
    public const string SectionName = "Bodacc";

    /// <summary>URL de base de l'API Opendatasoft (sans slash final).</summary>
    public string BaseUrl { get; set; } =
        "https://bodacc-datadila.opendatasoft.com/api/explore/v2.1/catalog/datasets/annonces-commerciales/records";

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Plafond du nombre d'annonces remontées par appel (sécurité).</summary>
    public int MaxResultsPerCall { get; set; } = 50;
}
