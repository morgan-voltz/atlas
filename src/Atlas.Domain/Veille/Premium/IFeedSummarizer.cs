using Atlas.Shared.Result;

namespace Atlas.Domain.Veille.Premium;

/// <summary>
/// Port premium (F-050) — produit une synthèse narrative d'un ensemble d'items de veille
/// (par exemple, un résumé hebdomadaire). Sert également de patron à <c>IFinancialSummarizer</c>
/// (F-054, synthèse narrative de la fiche financière). Une seule abstraction = une seule
/// frontière premium à câbler côté hébergé.
/// </summary>
public interface IFeedSummarizer
{
    Task<Result<string>> SummarizeAsync(IReadOnlyList<FeedItem> items, CancellationToken ct = default);
}
