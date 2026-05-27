using Atlas.Domain.Common;

namespace Atlas.Domain.Veille;

public static class VeilleErrors
{
    public static DomainError InvalidFeedSource(string reason) => new InvalidFeedSourceError(reason);

    public static readonly DomainError FetchFailed = new FeedFetchFailedError();

    public static readonly DomainError FeedUnreachable = new FeedUnreachableError();

    public static readonly DomainError SourceBlocked = new SourceBlockedError();

    public static readonly DomainError AlreadySubscribed = new AlreadySubscribedError();

    public static readonly DomainError SubscriptionNotFound = new SubscriptionNotFoundError();

    public static DomainError SubscriptionLimitReached(int max) => new SubscriptionLimitReachedError(max);

    public static DomainError InvalidVeillePack(string reason) => new InvalidVeillePackError(reason);

    public static readonly DomainError VeillePackNotFound = new VeillePackNotFoundError();

    public static readonly DomainError VeillePackNotEnrolled = new VeillePackNotEnrolledError();

    private sealed record InvalidFeedSourceError(string Reason)
        : DomainError("veille.invalid_feed_source", $"Source de veille invalide : {Reason}");

    private sealed record FeedFetchFailedError()
        : DomainError("veille.fetch_failed", "La récupération du flux a échoué.");

    private sealed record FeedUnreachableError()
        : DomainError("veille.feed_unreachable", "Le flux n'a pas pu être lu ou n'est pas un flux RSS/Atom valide.");

    private sealed record SourceBlockedError()
        : DomainError("veille.source_blocked", "Cette source n'est pas autorisée.");

    private sealed record AlreadySubscribedError()
        : DomainError("veille.already_subscribed", "Vous êtes déjà abonné à cette source.");

    private sealed record SubscriptionNotFoundError()
        : DomainError("veille.subscription_not_found", "Abonnement introuvable.");

    private sealed record SubscriptionLimitReachedError(int Max)
        : DomainError("veille.subscription_limit_reached", $"Limite d'abonnements atteinte ({Max}).");

    private sealed record InvalidVeillePackError(string Reason)
        : DomainError("veille.invalid_veille_pack", $"VeillePack invalide : {Reason}");

    private sealed record VeillePackNotFoundError()
        : DomainError("veille.veille_pack_not_found", "Pack de veille introuvable.");

    private sealed record VeillePackNotEnrolledError()
        : DomainError("veille.veille_pack_not_enrolled", "Vous n'avez pas appliqué ce pack de veille.");
}
