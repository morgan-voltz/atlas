using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille.Premium;

/// <summary>
/// Port premium (F-050) — score de pertinence d'un <see cref="FeedItem"/> pour un utilisateur
/// donné (0-100), tenant compte de ses favoris, abonnements et historique de lecture.
/// L'implémentation est confiée à un adapter d'infrastructure premium ; le cœur open source
/// reste agnostique des modèles utilisés (LLM, classifieur, heuristique pondérée…).
/// </summary>
public interface IFeedRelevanceScorer
{
    Task<Result<int>> ScoreAsync(FeedItem item, UserId userId, CancellationToken ct = default);
}
