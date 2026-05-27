using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetMySubscriptions;

internal sealed class GetMySubscriptionsHandler(
    IVeilleSubscriptionRepository subscriptionRepository,
    IFeedSourceRepository sourceRepository)
    : IRequestHandler<GetMySubscriptionsQuery, Result<IReadOnlyList<VeilleSubscriptionDto>>>
{
    public async Task<Result<IReadOnlyList<VeilleSubscriptionDto>>> Handle(
        GetMySubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

        IReadOnlyList<VeilleSubscription> subscriptions =
            await subscriptionRepository.GetByUserAsync(userId, cancellationToken);
        if (subscriptions.Count == 0)
        {
            return Result<IReadOnlyList<VeilleSubscriptionDto>>.Ok([]);
        }

        IReadOnlyCollection<FeedSourceId> sourceIds =
            subscriptions.Select(subscription => subscription.SourceId).Distinct().ToList();
        IReadOnlyList<FeedSource> sources = await sourceRepository.GetByIdsAsync(sourceIds, cancellationToken);
        Dictionary<FeedSourceId, FeedSource> sourcesById = sources.ToDictionary(source => source.Id);

        IReadOnlyList<VeilleSubscriptionDto> dtos = subscriptions
            .Where(subscription => sourcesById.ContainsKey(subscription.SourceId))
            .Select(subscription => VeilleSubscriptionDto.From(subscription, sourcesById[subscription.SourceId]))
            .ToList();

        return Result<IReadOnlyList<VeilleSubscriptionDto>>.Ok(dtos);
    }
}
