using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetRecentFeedItems;

internal sealed class GetRecentFeedItemsHandler(IFeedItemRepository itemRepository)
    : IRequestHandler<GetRecentFeedItemsQuery, Result<PagedResult<FeedItemDto>>>
{
    public async Task<Result<PagedResult<FeedItemDto>>> Handle(
        GetRecentFeedItemsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FeedItem> items = await itemRepository.GetRecentAsync(request.Page, request.PageSize, cancellationToken);
        long total = await itemRepository.CountAsync(cancellationToken);

        IReadOnlyList<FeedItemDto> dtos = items.Select(Map).ToList();
        return Result<PagedResult<FeedItemDto>>.Ok(
            new PagedResult<FeedItemDto>(dtos, request.Page, request.PageSize, total));
    }

    private static FeedItemDto Map(FeedItem item) => new(
        item.Id.Value,
        item.SourceId.Value,
        item.Title,
        item.Url,
        item.Summary,
        item.PublishedAt,
        string.IsNullOrEmpty(item.Categories) ? [] : item.Categories.Split('\n'));
}
