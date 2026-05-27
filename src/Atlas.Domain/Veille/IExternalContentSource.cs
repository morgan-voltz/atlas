using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

/// <summary>
/// Port de domaine : source externe de contenu daté (RSS/Atom, BODACC, BOPI…). Cf. doc 08 §9.1.
/// Chaque adapter déclare les types de source qu'il sait traiter et récupère les items publiés
/// depuis une date donnée.
/// </summary>
public interface IExternalContentSource
{
    bool CanHandle(FeedSourceType type);

    Task<Result<IReadOnlyList<FeedItemDraft>>> FetchAsync(
        FeedSource source,
        DateTimeOffset? since,
        CancellationToken ct = default);
}
