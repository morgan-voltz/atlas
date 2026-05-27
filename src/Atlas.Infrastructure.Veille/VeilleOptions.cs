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
}
