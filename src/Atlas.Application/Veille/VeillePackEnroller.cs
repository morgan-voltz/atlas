using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;

namespace Atlas.Application.Veille;

/// <summary>
/// Logique partagée entre l'application et la re-synchronisation d'un <see cref="VeillePack"/> (F-042) :
/// abonne l'utilisateur à toutes les sources du pack non encore suivies, puis met l'inscription à jour sur
/// la version courante. Ne consulte pas <see cref="IFeedSubscriptionPolicy"/> : les packs ignorent la limite
/// d'abonnements des ajouts libres (F-043).
/// </summary>
internal sealed class VeillePackEnroller(IVeilleSubscriptionRepository subscriptionRepository, IDateTimeProvider clock)
{
    /// <summary>Abonne aux sources manquantes et marque l'inscription synchronisée. Retourne le nb d'abonnements ajoutés.</summary>
    public async Task<int> BringUpToDateAsync(
        UserId userId,
        VeillePack pack,
        VeillePackEnrollment enrollment,
        CancellationToken ct)
    {
        DateTimeOffset now = clock.UtcNow;
        int added = 0;

        foreach (FeedSourceId sourceId in pack.SourceIds)
        {
            if (await subscriptionRepository.ExistsAsync(userId, sourceId, ct))
            {
                continue;
            }

            await subscriptionRepository.AddAsync(VeilleSubscription.Create(userId, sourceId, now), ct);
            added++;
        }

        enrollment.MarkSynced(pack.Version, now);
        return added;
    }
}
