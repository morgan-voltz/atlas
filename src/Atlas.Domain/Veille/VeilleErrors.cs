using Atlas.Domain.Common;

namespace Atlas.Domain.Veille;

public static class VeilleErrors
{
    public static DomainError InvalidFeedSource(string reason) => new InvalidFeedSourceError(reason);

    public static readonly DomainError FetchFailed = new FeedFetchFailedError();

    private sealed record InvalidFeedSourceError(string Reason)
        : DomainError("veille.invalid_feed_source", $"Source de veille invalide : {Reason}");

    private sealed record FeedFetchFailedError()
        : DomainError("veille.fetch_failed", "La récupération du flux a échoué.");
}
