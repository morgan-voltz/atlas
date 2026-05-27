namespace Atlas.Domain.Veille;

/// <summary>
/// Port de domaine : politique d'ajout libre de sources par l'utilisateur (F-043). Surface au use case
/// les règles configurables côté hébergeur : modération (URLs interdites) et limite d'abonnements par
/// compte (illimité en self-hosted, plafonné en hébergé). Implémenté dans l'infrastructure (cf. doc 02 §F-043).
/// </summary>
public interface IFeedSubscriptionPolicy
{
    /// <summary>Nombre maximal d'abonnements par utilisateur. <c>null</c> = illimité.</summary>
    int? MaxSubscriptionsPerUser { get; }

    /// <summary>Intervalle de polling appliqué à une source créée à partir d'un ajout libre.</summary>
    TimeSpan UserFeedPollingInterval { get; }

    /// <summary>Vrai si l'URL fournie par l'utilisateur n'est pas dans la liste de modération.</summary>
    bool IsUrlAllowed(string url);
}
