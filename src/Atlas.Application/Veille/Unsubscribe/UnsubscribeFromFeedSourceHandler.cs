using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Unsubscribe;

internal sealed class UnsubscribeFromFeedSourceHandler(
    IVeilleSubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnsubscribeFromFeedSourceCommand, Result>
{
    public async Task<Result> Handle(UnsubscribeFromFeedSourceCommand request, CancellationToken cancellationToken)
    {
        VeilleSubscription? subscription = await subscriptionRepository.GetAsync(
            new UserId(request.UserId),
            new VeilleSubscriptionId(request.SubscriptionId),
            cancellationToken);

        if (subscription is null)
        {
            return Result.Fail(VeilleErrors.SubscriptionNotFound);
        }

        subscriptionRepository.Remove(subscription);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
