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

    public static readonly DomainError FeedItemNotFound = new FeedItemNotFoundError();

    public static DomainError InvalidVeillePack(string reason) => new InvalidVeillePackError(reason);

    public static readonly DomainError VeillePackNotFound = new VeillePackNotFoundError();

    public static readonly DomainError VeillePackNotEnrolled = new VeillePackNotEnrolledError();

    public static DomainError InvalidFeedRule(string reason) => new InvalidFeedRuleError(reason);

    public static readonly DomainError FeedRuleNotFound = new FeedRuleNotFoundError();

    public static readonly DomainError FeedRuleForbidden = new FeedRuleForbiddenError();

    public static readonly DomainError VeillePackImmutable = new VeillePackImmutableError();

    public static readonly DomainError VeillePackForbidden = new VeillePackForbiddenError();

    public static readonly DomainError VeillePackNotPublic = new VeillePackNotPublicError();

    public static readonly DomainError VeillePackCodeAlreadyUsed = new VeillePackCodeAlreadyUsedError();

    public static DomainError InvalidVeillePackReport(string reason) => new InvalidVeillePackReportError(reason);

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

    private sealed record FeedItemNotFoundError()
        : DomainError("veille.feed_item_not_found", "Élément de veille introuvable.");

    private sealed record InvalidVeillePackError(string Reason)
        : DomainError("veille.invalid_veille_pack", $"VeillePack invalide : {Reason}");

    private sealed record VeillePackNotFoundError()
        : DomainError("veille.veille_pack_not_found", "Pack de veille introuvable.");

    private sealed record VeillePackNotEnrolledError()
        : DomainError("veille.veille_pack_not_enrolled", "Vous n'avez pas appliqué ce pack de veille.");

    private sealed record InvalidFeedRuleError(string Reason)
        : DomainError("veille.invalid_feed_rule", $"Règle de surveillance invalide : {Reason}");

    private sealed record FeedRuleNotFoundError()
        : DomainError("veille.feed_rule_not_found", "Règle de surveillance introuvable.");

    private sealed record FeedRuleForbiddenError()
        : DomainError("veille.feed_rule_forbidden", "Vous n'êtes pas autorisé à modifier cette règle.");

    private sealed record VeillePackImmutableError()
        : DomainError("veille.veille_pack_immutable", "Un pack système ne peut pas être modifié par l'utilisateur.");

    private sealed record VeillePackForbiddenError()
        : DomainError("veille.veille_pack_forbidden", "Vous n'êtes pas autorisé à modifier ce pack.");

    private sealed record VeillePackNotPublicError()
        : DomainError("veille.veille_pack_not_public", "Ce pack n'est pas publié au marketplace communautaire.");

    private sealed record VeillePackCodeAlreadyUsedError()
        : DomainError("veille.veille_pack_code_already_used", "Ce code de pack est déjà utilisé.");

    private sealed record InvalidVeillePackReportError(string Reason)
        : DomainError("veille.invalid_veille_pack_report", $"Signalement invalide : {Reason}");
}
