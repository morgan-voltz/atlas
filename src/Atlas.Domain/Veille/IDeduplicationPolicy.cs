namespace Atlas.Domain.Veille;

/// <summary>
/// Port de domaine : paramètres configurables de la déduplication intelligente (F-045), implémenté dans
/// l'infrastructure à partir des options de veille (cf. <see cref="IFeedSubscriptionPolicy"/>).
/// </summary>
public interface IDeduplicationPolicy
{
    /// <summary>Distance de Hamming maximale (sur 64 bits) en deçà de laquelle deux empreintes sont jugées similaires.</summary>
    int MaxHammingDistance { get; }

    /// <summary>Fenêtre temporelle autour de la date de publication dans laquelle deux items peuvent être regroupés.</summary>
    TimeSpan ClusterWindow { get; }
}
