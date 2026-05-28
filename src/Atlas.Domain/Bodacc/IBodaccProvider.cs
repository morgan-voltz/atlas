using Atlas.Domain.Companies;
using Atlas.Shared.Result;

namespace Atlas.Domain.Bodacc;

/// <summary>
/// Port de lecture des annonces BODACC publiques (F-048).
/// L'API est anonyme (data.gouv.fr / Opendatasoft) — pas d'authentification utilisateur requise,
/// contrairement à l'INPI. Limite de débit gérée côté adapter.
/// </summary>
public interface IBodaccProvider
{
    /// <summary>
    /// Renvoie les annonces publiées pour un SIREN depuis <paramref name="since"/>, ordonnées par date
    /// décroissante, plafonnées à <paramref name="limit"/>.
    /// </summary>
    Task<Result<IReadOnlyList<BodaccAnnouncement>>> GetAnnouncementsAsync(
        Siren siren,
        DateTimeOffset since,
        int limit,
        CancellationToken ct = default);
}
