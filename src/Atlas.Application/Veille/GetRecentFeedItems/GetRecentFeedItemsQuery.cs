using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetRecentFeedItems;

public sealed record GetRecentFeedItemsQuery(int Page, int PageSize)
    : IRequest<Result<PagedResult<FeedItemDto>>>;

public sealed record FeedItemDto(
    Guid Id,
    Guid SourceId,
    string Title,
    string? Url,
    string? Summary,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> Categories);
