using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetTimeline;

internal sealed class GetTimelineHandler(IFeedItemRepository itemRepository)
    : IRequestHandler<GetTimelineQuery, Result<PagedResult<TimelineItemDto>>>
{
    public async Task<Result<PagedResult<TimelineItemDto>>> Handle(
        GetTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new TimelineFilter(
            request.SourceId is { } sourceId ? new FeedSourceId(sourceId) : null,
            request.After,
            request.Before,
            string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim(),
            request.UnreadOnly,
            request.FavoritesOnly,
            request.IncludeArchived);

        PagedResult<TimelineEntry> page = await itemRepository.GetTimelineAsync(
            new UserId(request.UserId), filter, request.Page, request.PageSize, cancellationToken);

        IReadOnlyList<TimelineItemDto> dtos = page.Items.Select(Map).ToList();
        return Result<PagedResult<TimelineItemDto>>.Ok(
            new PagedResult<TimelineItemDto>(dtos, page.Page, page.PageSize, page.TotalCount));
    }

    private static TimelineItemDto Map(TimelineEntry entry) => new(
        entry.Item.Id.Value,
        entry.Item.SourceId.Value,
        entry.Item.Title,
        entry.Item.Url,
        entry.Item.Summary,
        entry.Item.PublishedAt,
        string.IsNullOrEmpty(entry.Item.Categories) ? [] : entry.Item.Categories.Split('\n'),
        entry.IsRead,
        entry.IsFavorite,
        entry.IsArchived,
        entry.SourceCount);
}
