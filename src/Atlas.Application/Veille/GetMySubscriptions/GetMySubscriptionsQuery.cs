using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetMySubscriptions;

public sealed record GetMySubscriptionsQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<VeilleSubscriptionDto>>>;

public sealed record VeilleSubscriptionDto(
    Guid SubscriptionId,
    Guid SourceId,
    string SourceName,
    string SourceUrl,
    string SourceType,
    bool IsActive,
    DateTimeOffset SubscribedAt)
{
    internal static VeilleSubscriptionDto From(VeilleSubscription subscription, FeedSource source) => new(
        subscription.Id.Value,
        source.Id.Value,
        source.Name,
        source.Url,
        source.Type.ToString(),
        source.IsActive,
        subscription.CreatedAt);
}
