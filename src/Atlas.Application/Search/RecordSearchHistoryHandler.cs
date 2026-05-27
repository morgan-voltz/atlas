using Atlas.Domain.Common;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using MediatR;

namespace Atlas.Application.Search;

internal sealed class RecordSearchHistoryHandler(
    ISearchHistoryRepository searchHistoryRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : INotificationHandler<SearchPerformedNotification>
{
    private const int MaxEntriesPerUser = 200;

    public async Task Handle(SearchPerformedNotification notification, CancellationToken cancellationToken)
    {
        var userId = new UserId(notification.UserId);

        SearchHistoryEntry entry = SearchHistoryEntry.Record(userId, notification.Type, notification.Query, clock.UtcNow);
        await searchHistoryRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await searchHistoryRepository.PruneAsync(userId, MaxEntriesPerUser, cancellationToken);
    }
}
