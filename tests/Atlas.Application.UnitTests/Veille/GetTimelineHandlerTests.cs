using Atlas.Application.Veille.GetTimeline;
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

    private GetTimelineHandler CreateHandler() => new(_itemRepo);

    [Fact]
    public async Task Handle_passes_filter_and_maps_entries_with_flags()
    {
        var userId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        FeedItem item = FeedItem.Create(
            new FeedSourceId(sourceId), "Titre", "https://x.test/1", "résumé", Now, ["tech", "veille"], Now);
        var entry = new TimelineEntry(item, IsRead: true, IsFavorite: false, IsArchived: false, SourceCount: 3,
            MentionedFavorites: [new FavoriteMention("552032534", "Renault")]);

        TimelineFilter? captured = null;
        _itemRepo.GetTimelineAsync(Arg.Any<UserId>(), Arg.Do<TimelineFilter>(f => captured = f),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<TimelineEntry>([entry], 1, 20, 1));

        var query = new GetTimelineQuery(userId, 1, 20, sourceId, null, null, "veille", true, false, false, false);
        Result<PagedResult<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        TimelineItemDto dto = result.Value!.Items.Should().ContainSingle().Subject;
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
    public async Task Handle_propagates_pagination()
    {
        _itemRepo.GetTimelineAsync(Arg.Any<UserId>(), Arg.Any<TimelineFilter>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<TimelineEntry>([], 2, 10, 42));

        var query = new GetTimelineQuery(Guid.NewGuid(), 2, 10, null, null, null, null, false, false, false, false);
        Result<PagedResult<TimelineItemDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Page.Should().Be(2);
        result.Value!.PageSize.Should().Be(10);
        result.Value!.TotalCount.Should().Be(42);
    }
}
