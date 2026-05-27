using Atlas.Application.Veille.PollFeedSources;
using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class PollFeedSourcesHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_persists_only_new_items_and_marks_source_polled()
    {
        FeedSource source = CreateDueSource();

        IFeedSourceRepository sourceRepo = Substitute.For<IFeedSourceRepository>();
        sourceRepo.GetActiveAsync(Arg.Any<CancellationToken>()).Returns(new[] { source });

        var drafts = new List<FeedItemDraft>
        {
            new("Article 1", "https://x.test/1", "d1", Now, []),
            new("Article 2", "https://x.test/2", "d2", Now, []),
        };
        string existingHash = FeedItem.ComputeHash("https://x.test/1", "Article 1");

        IExternalContentSource provider = Substitute.For<IExternalContentSource>();
        provider.CanHandle(FeedSourceType.Rss).Returns(true);
        provider.FetchAsync(Arg.Any<FeedSource>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FeedItemDraft>>.Ok(drafts));

        IFeedItemRepository itemRepo = Substitute.For<IFeedItemRepository>();
        itemRepo.GetExistingHashesAsync(source.Id, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<string> { existingHash });

        IDateTimeProvider clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(Now);
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new PollFeedSourcesHandler(sourceRepo, itemRepo, new[] { provider }, clock, unitOfWork);

        Result<FeedPollSummary> result = await handler.Handle(new PollFeedSourcesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SourcesPolled.Should().Be(1);
        result.Value!.ItemsAdded.Should().Be(1);
        result.Value!.SourcesFailed.Should().Be(0);
        source.LastPolledAt.Should().Be(Now);

        await itemRepo.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<FeedItem>>(items => items.Count() == 1 && items.First().Title == "Article 2"),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_counts_failure_when_provider_fetch_fails_but_still_marks_polled()
    {
        FeedSource source = CreateDueSource();

        IFeedSourceRepository sourceRepo = Substitute.For<IFeedSourceRepository>();
        sourceRepo.GetActiveAsync(Arg.Any<CancellationToken>()).Returns(new[] { source });

        IExternalContentSource provider = Substitute.For<IExternalContentSource>();
        provider.CanHandle(FeedSourceType.Rss).Returns(true);
        provider.FetchAsync(Arg.Any<FeedSource>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FeedItemDraft>>.Fail(VeilleErrors.FetchFailed));

        IFeedItemRepository itemRepo = Substitute.For<IFeedItemRepository>();
        IDateTimeProvider clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(Now);
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new PollFeedSourcesHandler(sourceRepo, itemRepo, new[] { provider }, clock, unitOfWork);

        Result<FeedPollSummary> result = await handler.Handle(new PollFeedSourcesCommand(), CancellationToken.None);

        result.Value!.SourcesFailed.Should().Be(1);
        result.Value!.ItemsAdded.Should().Be(0);
        source.LastPolledAt.Should().Be(Now);
        await itemRepo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<FeedItem>>(), Arg.Any<CancellationToken>());
    }

    private static FeedSource CreateDueSource() =>
        FeedSource.Create("Src", "https://x.test/feed", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now.AddHours(-1)).Value!;
}
