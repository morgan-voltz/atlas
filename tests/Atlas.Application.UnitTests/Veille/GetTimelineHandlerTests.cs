using Atlas.Application.Veille.GetTimeline;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class GetTimelineHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IFeedItemRepository _itemRepo = Substitute.For<IFeedItemRepository>();
    private readonly IFavoriteEventRepository _eventRepo = Substitute.For<IFavoriteEventRepository>();

    private GetTimelineHandler CreateHandler() => new(_itemRepo, _eventRepo);

    private void StubItems(params TimelineEntry[] entries) =>
        _itemRepo.GetTimelineAsync(Arg.Any<UserId>(), Arg.Any<TimelineFilter>(),
                Arg.Any<TimelineCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<TimelineEntry>)entries);

    private void StubEvents(params FavoriteEvent[] events) =>
        _eventRepo.GetForUserAsync(Arg.Any<UserId>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
                Arg.Any<TimelineCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<FavoriteEvent>)events);

    private static TimelineEntry RssEntry(DateTimeOffset publishedAt, int sourceCount = 1) =>
        new(
            FeedItem.Create(new FeedSourceId(Guid.NewGuid()), "Titre", $"https://x.test/{Guid.NewGuid()}", null, publishedAt, null, Now),
            IsRead: false, IsFavorite: false, IsArchived: false, sourceCount, MentionedFavorites: []);

    [Fact]
    public async Task Handle_passes_filter_and_maps_rss_entries()
    {
        var userId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        FeedItem item = FeedItem.Create(
            new FeedSourceId(sourceId), "Titre", "https://x.test/1", "résumé", Now, ["tech", "veille"], Now);
        var entry = new TimelineEntry(item, IsRead: true, IsFavorite: false, IsArchived: false, SourceCount: 3,
            MentionedFavorites: [new FavoriteMention("552032534", "Renault")]);

        TimelineFilter? captured = null;
        _itemRepo.GetTimelineAsync(Arg.Any<UserId>(), Arg.Do<TimelineFilter>(f => captured = f),
                Arg.Any<TimelineCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<TimelineEntry>)[entry]);
        StubEvents();

        // SourceId positionné → events exclus (filtre RSS-only).
        var query = new GetTimelineQuery(userId, null, 20, sourceId, null, null, "veille", true, false, false, false);
        Result<CursorPage<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        TimelineItemDto dto = result.Value!.Items.Should().ContainSingle().Subject;
        dto.Kind.Should().Be("RssItem");
        dto.Title.Should().Be("Titre");
        dto.IsRead.Should().BeTrue();
        dto.SourceCount.Should().Be(3);
        dto.Categories.Should().BeEquivalentTo(["tech", "veille"]);
        dto.MentionedFavorites.Should().HaveCount(1).And.Subject.First().Should().BeEquivalentTo(
            new FavoriteMentionDto("552032534", "Renault"));

        captured.Should().NotBeNull();
        captured!.SourceId.Should().Be(new FeedSourceId(sourceId));
        captured.Keyword.Should().Be("veille");
        captured.UnreadOnly.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_merges_rss_and_favorite_events_in_chronological_order()
    {
        UserId userId = UserId.New();
        Siren siren = Siren.Create("552032534").Value;
        var rssEntry = RssEntry(Now.AddHours(-2));

        FavoriteEvent evt = FavoriteEvent.Record(
            userId, siren, FavoriteEventType.RneChanged, "Mise à jour de Renault", "Dénomination modifiée.", Now);

        StubItems(rssEntry);
        StubEvents(evt);

        var query = new GetTimelineQuery(userId.Value, null, 20, null, null, null, null, false, false, false, false);
        Result<CursorPage<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        // L'event (Now) doit être plus récent que l'article RSS (Now - 2h).
        result.Value!.Items[0].Kind.Should().Be("FavoriteEvent");
        result.Value!.Items[0].EventType.Should().Be("RneChanged");
        result.Value!.Items[0].EventSiren.Should().Be("552032534");
        result.Value!.Items[1].Kind.Should().Be("RssItem");
    }

    [Fact]
    public async Task Handle_excludes_events_when_unread_only_filter_active()
    {
        // UnreadOnly est RSS-only (les events n'ont pas d'état lu) → events ignorés.
        StubItems();

        var query = new GetTimelineQuery(Guid.NewGuid(), null, 20, null, null, null, null,
            UnreadOnly: true, FavoritesOnly: false, IncludeArchived: false, MentionsFavoritesOnly: false);

        await CreateHandler().Handle(query, CancellationToken.None);

        await _eventRepo.DidNotReceive().GetForUserAsync(
            Arg.Any<UserId>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<TimelineCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_excludes_events_when_editorial_only_even_without_rss_filter()
    {
        // Veille (doc 12 §6) : editorialOnly=true exclut les FavoriteEvent même si AUCUN filtre RSS n'est actif.
        StubItems();

        var query = new GetTimelineQuery(Guid.NewGuid(), null, 20, null, null, null, null,
            UnreadOnly: false, FavoritesOnly: false, IncludeArchived: false, MentionsFavoritesOnly: false,
            EditorialOnly: true);

        Result<CursorPage<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _eventRepo.DidNotReceive().GetForUserAsync(
            Arg.Any<UserId>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(),
            Arg.Any<TimelineCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_next_cursor_when_more_than_page_size()
    {
        // Le repo renvoie pageSize+1 entrées → il reste une page : on tronque à pageSize et on émet un curseur.
        StubItems(RssEntry(Now), RssEntry(Now.AddMinutes(-1)), RssEntry(Now.AddMinutes(-2)));
        StubEvents();

        var query = new GetTimelineQuery(Guid.NewGuid(), null, 2, null, null, null, null, false, false, false, false);
        Result<CursorPage<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.HasMore.Should().BeTrue();
        result.Value!.NextCursor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_has_no_next_cursor_when_exhausted()
    {
        StubItems(RssEntry(Now));
        StubEvents();

        var query = new GetTimelineQuery(Guid.NewGuid(), null, 20, null, null, null, null, false, false, false, false);
        Result<CursorPage<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value!.HasMore.Should().BeFalse();
        result.Value!.NextCursor.Should().BeNull();
    }
}
