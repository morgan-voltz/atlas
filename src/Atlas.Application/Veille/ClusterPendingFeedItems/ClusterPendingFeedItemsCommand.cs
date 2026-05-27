using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.ClusterPendingFeedItems;

/// <summary>Rattache les items pas encore clusterisés à un cluster de déduplication floue, ou en crée un (F-045).</summary>
public sealed record ClusterPendingFeedItemsCommand : IRequest<Result<ClusterRunSummary>>;

public sealed record ClusterRunSummary(int ItemsProcessed, int ClustersCreated, int ItemsClustered);
