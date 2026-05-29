using Atlas.Shared.Result;

namespace Atlas.Domain.Veille.Premium;

/// <summary>
/// Port premium (F-050) — enrichit un <see cref="FeedItem"/> avec des données dérivées
/// produites par un LLM ou un service externe (catégorisation, extraction d'entités,
/// résumé court par item). Implémenté côté <c>Atlas.Infrastructure.*</c> futur, jamais
/// dans le cœur open source. Le call-site vit dans <c>Atlas.Application.Premium</c>.
/// </summary>
public interface IFeedItemEnricher
{
    Task<Result<FeedItemEnrichment>> EnrichAsync(FeedItem item, CancellationToken ct = default);
}

/// <summary>
/// Résultat d'un enrichissement (F-050). Champs optionnels : un adapter peut ne renseigner
/// que ce qu'il sait faire (par ex. classifier mais pas résumer).
/// </summary>
public sealed record FeedItemEnrichment(
    IReadOnlyList<string> Topics,
    string? ShortSummary);
