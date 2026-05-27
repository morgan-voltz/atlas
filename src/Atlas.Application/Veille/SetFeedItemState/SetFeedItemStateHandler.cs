using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.SetFeedItemState;

internal sealed class SetFeedItemStateHandler(
    IFeedItemRepository itemRepository,
    IFeedItemUserStateRepository stateRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetFeedItemStateCommand, Result<FeedItemStateDto>>
{
    public async Task<Result<FeedItemStateDto>> Handle(
        SetFeedItemStateCommand request,
        CancellationToken cancellationToken)
    {
        var feedItemId = new FeedItemId(request.FeedItemId);
        if (!await itemRepository.ExistsAsync(feedItemId, cancellationToken))
        {
            return Result<FeedItemStateDto>.Fail(VeilleErrors.FeedItemNotFound);
        }

        var userId = new UserId(request.UserId);
        DateTimeOffset now = clock.UtcNow;

        FeedItemUserState? state = await stateRepository.GetAsync(userId, feedItemId, cancellationToken);
        bool isNew = state is null;
        state ??= FeedItemUserState.Create(userId, feedItemId, now);

        if (request.IsRead is { } isRead)
        {
            state.SetRead(isRead, now);
        }

        if (request.IsFavorite is { } isFavorite)
        {
            state.SetFavorite(isFavorite, now);
        }

        if (request.IsArchived is { } isArchived)
        {
            state.SetArchived(isArchived, now);
        }

        if (isNew)
        {
            await stateRepository.AddAsync(state, cancellationToken);
        }
        else
        {
            stateRepository.Update(state);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FeedItemStateDto>.Ok(
            new FeedItemStateDto(feedItemId.Value, state.IsRead, state.IsFavorite, state.IsArchived));
    }
}
