using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetTimeline;

/// <summary>
/// Timeline unifiée (F-044 + F-047 volet 2) : items RSS des sources abonnées du user + événements
/// (changements RNE) sur ses favoris, fusionnés et triés chronologiquement.
/// Pagination keyset (curseur opaque) : chaque flux ne renvoie que <c>pageSize+1</c> entrées situées
/// après le curseur, supprimant l'ancien plafond de fusion et le sur-fetch. L'ordre total est
/// <c>(OccurredAt DESC, Id DESC)</c>, le départage par Id garantissant l'absence de saut entre pages.
/// </summary>
internal sealed class GetTimelineHandler(
    IFeedItemRepository itemRepository,
    IFavoriteEventRepository eventRepository)
    : IRequestHandler<GetTimelineQuery, Result<CursorPage<TimelineItemDto>>>
{
    // Départage des ex-aequo d'horodatage, aligné sur l'ordre uuid PostgreSQL utilisé par les repositories.
    private static readonly IComparer<Guid> GuidComparer = Comparer<Guid>.Create(TimelineKeyset.CompareGuid);

    public async Task<Result<CursorPage<TimelineItemDto>>> Handle(
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
        TimelineCursor? cursor = TimelineCursorCodec.Decode(request.Cursor);

        // Keyset : on demande pageSize+1 de CHAQUE flux. Le top (pageSize+1) de l'union est forcément
        // inclus dans l'union des deux top-(pageSize+1), donc la fusion est exacte. Le +1 sert à détecter
        // s'il reste une page suivante.
        int take = request.PageSize + 1;

        IReadOnlyList<TimelineEntry> rss = await itemRepository.GetTimelineAsync(
            userId, filter, cursor, take, cancellationToken);

        // Événements RNE : exclus si l'un des filtres RSS-spécifiques est actif (les events n'ont pas de
        // source RSS ni d'état utilisateur) ou si l'appelant demande le contenu éditorial seul (doc 12 §6).
        IReadOnlyList<FavoriteEvent> events = !request.EditorialOnly && ShouldIncludeEvents(filter)
            ? await eventRepository.GetForUserAsync(
                userId, request.After, request.Before, cursor, take, cancellationToken)
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

        // Fusion des deux flux selon le même ordre total que les repositories : (OccurredAt DESC, Id DESC).
        var merged = rss.Select(MapRss)
            .Concat(events.Select(MapEvent))
            .OrderByDescending(item => item.OccurredAt)
            .ThenByDescending(item => item.Id, GuidComparer)
            .Take(take)
            .ToList();

        bool hasMore = merged.Count > request.PageSize;
        List<TimelineItemDto> page = hasMore ? merged.GetRange(0, request.PageSize) : merged;

        string? nextCursor = hasMore
            ? TimelineCursorCodec.Encode(new TimelineCursor(page[^1].OccurredAt, page[^1].Id))
            : null;

        return Result<CursorPage<TimelineItemDto>>.Ok(new CursorPage<TimelineItemDto>(page, nextCursor));
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
