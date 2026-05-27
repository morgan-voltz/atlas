using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.ClusterPendingFeedItems;

internal sealed class ClusterPendingFeedItemsHandler(
    IFeedItemRepository itemRepository,
    IFeedItemClusterRepository clusterRepository,
    IDeduplicationPolicy policy,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<ClusterPendingFeedItemsCommand, Result<ClusterRunSummary>>
{
    private const int BatchSize = 500;

    public async Task<Result<ClusterRunSummary>> Handle(
        ClusterPendingFeedItemsCommand request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FeedItem> items = await itemRepository.GetUnclusteredAsync(BatchSize, cancellationToken);
        if (items.Count == 0)
        {
            return Result<ClusterRunSummary>.Ok(new ClusterRunSummary(0, 0, 0));
        }

        DateTimeOffset now = clock.UtcNow;
        TimeSpan window = policy.ClusterWindow;

        // Candidats d'appariement : clusters dont l'activité chevauche la fenêtre du lot, plus ceux créés au fil du lot.
        DateTimeOffset since = items.Min(item => item.PublishedAt) - window;
        var candidates = (await clusterRepository.GetActiveSinceAsync(since, cancellationToken)).ToList();

        // Clusters créés dans ce lot : déjà suivis en état « Added » par EF, leurs mutations seront persistées
        // à l'insertion. Appeler Update dessus les ferait basculer en « Modified » et l'insert serait perdu.
        var createdIds = new HashSet<FeedItemClusterId>();

        int clustersCreated = 0;
        int itemsClustered = 0;

        foreach (FeedItem item in items)
        {
            long fingerprint = SimHash.Compute($"{item.Title} {item.Summary}");

            FeedItemCluster? best = null;
            int bestDistance = int.MaxValue;
            foreach (FeedItemCluster candidate in candidates)
            {
                if (item.PublishedAt < candidate.FirstPublishedAt - window
                    || item.PublishedAt > candidate.LastPublishedAt + window)
                {
                    continue;
                }

                int distance = SimHash.HammingDistance(fingerprint, candidate.SimHash);
                if (distance <= policy.MaxHammingDistance && distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            if (best is not null)
            {
                item.AttachToCluster(best.Id);
                best.AddItem(item);
                if (!createdIds.Contains(best.Id))
                {
                    clusterRepository.Update(best);
                }

                itemsClustered++;
            }
            else
            {
                var created = FeedItemCluster.Create(fingerprint, item, now);
                item.AttachToCluster(created.Id);
                await clusterRepository.AddAsync(created, cancellationToken);
                candidates.Add(created);
                createdIds.Add(created.Id);
                clustersCreated++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ClusterRunSummary>.Ok(new ClusterRunSummary(items.Count, clustersCreated, itemsClustered));
    }
}
