namespace Atlas.Infrastructure.Veille;

public sealed class VeilleOptions
{
    public const string SectionName = "Veille";

    /// <summary>Timeout HTTP pour récupérer un flux.</summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>En-tête User-Agent envoyé aux serveurs de flux.</summary>
    public string UserAgent { get; set; } = "AtlasVeille/1.0 (+https://github.com/Morgan-Voltz/atlas)";

    /// <summary>Intervalle de polling appliqué aux sources ajoutées librement par les utilisateurs (F-043).</summary>
    public int UserFeedPollingMinutes { get; set; } = 30;

    /// <summary>
    /// Limite d'abonnements par utilisateur (F-043). <c>null</c> ou valeur ≤ 0 = illimité (cas self-hosted).
    /// </summary>
    public int? MaxSubscriptionsPerUser { get; set; } = 100;

    /// <summary>
    /// Bibliothèque de fragments d'hôtes interdits (spam, contenu manifestement illégal). La comparaison
    /// est insensible à la casse sur l'hôte de l'URL fournie par l'utilisateur (F-043, modération).
    /// </summary>
    public IList<string> BlockedHostFragments { get; set; } = [];

    /// <summary>
    /// Seuil de similarité (0..1) au-delà duquel deux items sont regroupés en un cluster de déduplication (F-045).
    /// Par défaut 0,8 → distance de Hamming max ≈ 12 bits sur 64.
    /// </summary>
    public double DeduplicationThreshold { get; set; } = 0.8;

    /// <summary>Fenêtre temporelle (heures) autour de la publication dans laquelle deux items peuvent être regroupés (F-045).</summary>
    public int ClusterWindowHours { get; set; } = 72;
}
