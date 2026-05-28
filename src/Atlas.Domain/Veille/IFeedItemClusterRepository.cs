namespace Atlas.Domain.Veille;

public interface IFeedItemClusterRepository
{
    /// <summary>Clusters dont le dernier item publié est postérieur à <paramref name="since"/> (candidats d'appariement, F-045).</summary>
    Task<IReadOnlyList<FeedItemCluster>> GetActiveSinceAsync(DateTimeOffset since, CancellationToken ct = default);

    Task AddAsync(FeedItemCluster cluster, CancellationToken ct = default);

    void Update(FeedItemCluster cluster);
}
