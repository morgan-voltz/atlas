using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetTimeline;

/// <summary>
/// Timeline unifiée (F-044 + F-047 volet 2) : items RSS des sources abonnées du user + événements
/// (changements RNE) sur ses favoris, fusionnés et triés chronologiquement.
/// Pour MVP, la fusion est bornée à <see cref="FusionBuffer"/> entrées de chaque source ; au-delà,
/// les très anciens items peuvent être tronqués. Une vue matérialisée SQL pourra remplacer cette
/// approche si le volume l'exige.
/// </summary>
internal sealed class GetTimelineHandler(
    IFeedItemRepository itemRepository,
    IFavoriteEventRepository eventRepository)
    : IRequestHandler<GetTimelineQuery, Result<PagedResult<TimelineItemDto>>>
{
    private const int FusionBuffer = 500;

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
            request.IncludeArchived,
            request.MentionsFavoritesOnly);

        var userId = new UserId(request.UserId);

        // RSS items (buffer largement supérieur à pageSize pour permettre la fusion correcte).
        PagedResult<TimelineEntry> rssPage = await itemRepository.GetTimelineAsync(
            userId, filter, page: 1, pageSize: FusionBuffer, cancellationToken);

        // Événements RNE : exclus si l'un des filtres RSS-spécifiques est actif (les events n'ont
        // pas de source RSS ni d'état utilisateur — sinon ils apparaîtraient toujours, surprise).
        IReadOnlyList<FavoriteEvent> events = ShouldIncludeEvents(filter)
            ? await eventRepository.GetForUserAsync(
                userId, request.After, request.Before, FusionBuffer, cancellationToken)
            : [];

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            string keyword = filter.Keyword;
            events = events
                .Where(e =>
                    e.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || (e.Summary?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        IEnumerable<TimelineItemDto> rssDtos = rssPage.Items.Select(MapRss);
        IEnumerable<TimelineItemDto> eventDtos = events.Select(MapEvent);

        var merged = rssDtos
            .Concat(eventDtos)
            .OrderByDescending(item => item.OccurredAt)
            .ToList();

        // TotalCount approximé : somme du total RSS + nombre d'events chargés.
        long total = rssPage.TotalCount + events.Count;

        var paged = merged
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<PagedResult<TimelineItemDto>>.Ok(
            new PagedResult<TimelineItemDto>(paged, request.Page, request.PageSize, total));
    }

    /// <summary>
    /// Les events n'ont pas d'<c>IsRead/IsFavorite/IsArchived</c> ni de source RSS, donc on les exclut
    /// si l'un de ces filtres est positionné — sinon ils apparaîtraient toujours, ce qui surprendrait.
    /// </summary>
    private static bool ShouldIncludeEvents(TimelineFilter filter) =>
        filter.SourceId is null
        && !filter.UnreadOnly
        && !filter.FavoritesOnly
        && !filter.MentionsFavoritesOnly;

    private static TimelineItemDto MapRss(TimelineEntry entry) => new(
        Kind: "RssItem",
        Id: entry.Item.Id.Value,
        Title: entry.Item.Title,
        Url: entry.Item.Url,
        Summary: entry.Item.Summary,
        OccurredAt: entry.Item.PublishedAt,
        IsRead: entry.IsRead,
        IsFavorite: entry.IsFavorite,
        IsArchived: entry.IsArchived,
        SourceId: entry.Item.SourceId.Value,
        Categories: string.IsNullOrEmpty(entry.Item.Categories) ? [] : entry.Item.Categories.Split('\n'),
        SourceCount: entry.SourceCount,
        MentionedFavorites: entry.MentionedFavorites
            .Select(m => new FavoriteMentionDto(m.Siren, m.Name))
            .ToList(),
        EventType: null,
        EventSiren: null);

    private static TimelineItemDto MapEvent(FavoriteEvent evt) => new(
        Kind: "FavoriteEvent",
        Id: evt.Id.Value,
        Title: evt.Title,
        Url: null,
        Summary: evt.Summary,
        OccurredAt: evt.OccurredAt,
        IsRead: false,
        IsFavorite: false,
        IsArchived: false,
        SourceId: null,
        Categories: [],
        SourceCount: 1,
        MentionedFavorites: [],
        EventType: evt.Type.ToString(),
        EventSiren: evt.Siren.Value);
}
