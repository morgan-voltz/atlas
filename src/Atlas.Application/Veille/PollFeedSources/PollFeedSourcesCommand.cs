using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.PollFeedSources;

/// <summary>Déclenche le polling des sources de veille actives « dues » (F-041).</summary>
public sealed record PollFeedSourcesCommand : IRequest<Result<FeedPollSummary>>;

public sealed record FeedPollSummary(int SourcesPolled, int ItemsAdded, int SourcesFailed);
