using Atlas.Application.Veille.ClusterPendingFeedItems;
using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class ClusterPendingFeedItemsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    // Deux variantes normalisées-identiques (casse/ponctuation) → distance de Hamming 0, regroupement garanti.
    private const string TitleA = "Le groupe Atlas rachète la startup Beta pour 50 millions d'euros";
    private const string TitleAVariant = "LE GROUPE ATLAS rachète la startup Beta pour 50 Millions d'euros !!!";
    private const string TitleUnrelated = "La météo sera pluvieuse sur la côte atlantique ce week-end";

    private readonly IFeedItemRepository _itemRepo = Substitute.For<IFeedItemRepository>();
    private readonly IFeedItemClusterRepository _clusterRepo = Substitute.For<IFeedItemClusterRepository>();
    private readonly IDeduplicationPolicy _policy = Substitute.For<IDeduplicationPolicy>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public ClusterPendingFeedItemsHandlerTests()
    {
        _policy.MaxHammingDistance.Returns(12);
        _policy.ClusterWindow.Returns(TimeSpan.FromHours(72));
        _clock.UtcNow.Returns(Now);
        _clusterRepo.GetActiveSinceAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<FeedItemCluster>());
    }

    [Fact]
    public async Task Near_identical_items_from_distinct_sources_share_one_cluster()
    {
        FeedItem older = Item(TitleA, Now.AddHours(-2));
        FeedItem newer = Item(TitleAVariant, Now.AddHours(-1));
        GivenUnclustered(older, newer);

        ClusterRunSummary summary = await Run();

        summary.ClustersCreated.Should().Be(1);
        summary.ItemsClustered.Should().Be(1);
        older.ClusterId.Should().NotBeNull();
        newer.ClusterId.Should().Be(older.ClusterId);
        await _clusterRepo.Received(1).AddAsync(Arg.Any<FeedItemCluster>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unrelated_items_form_separate_clusters()
    {
        FeedItem first = Item(TitleA, Now.AddHours(-2));
        FeedItem second = Item(TitleUnrelated, Now.AddHours(-1));
        GivenUnclustered(first, second);

        ClusterRunSummary summary = await Run();

        summary.ClustersCreated.Should().Be(2);
        summary.ItemsClustered.Should().Be(0);
        first.ClusterId.Should().NotBe(second.ClusterId);
        await _clusterRepo.Received(2).AddAsync(Arg.Any<FeedItemCluster>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Near_identical_items_outside_the_window_are_not_grouped()
    {
        FeedItem older = Item(TitleA, Now.AddHours(-200));
        FeedItem newer = Item(TitleAVariant, Now.AddHours(-1));
        GivenUnclustered(older, newer);

        ClusterRunSummary summary = await Run();

        summary.ClustersCreated.Should().Be(2);
        summary.ItemsClustered.Should().Be(0);
        newer.ClusterId.Should().NotBe(older.ClusterId);
    }

    [Fact]
    public async Task Near_identical_items_from_the_same_source_are_still_grouped()
    {
        FeedSourceId source = FeedSourceId.New();
        FeedItem older = Item(TitleA, Now.AddHours(-2), source);
        FeedItem newer = Item(TitleAVariant, Now.AddHours(-1), source);
        GivenUnclustered(older, newer);

        ClusterRunSummary summary = await Run();

        summary.ClustersCreated.Should().Be(1);
        summary.ItemsClustered.Should().Be(1);
        newer.ClusterId.Should().Be(older.ClusterId);
    }

    [Fact]
    public async Task No_pending_items_returns_an_empty_summary_without_saving()
    {
        GivenUnclustered();

        ClusterRunSummary summary = await Run();

        summary.Should().Be(new ClusterRunSummary(0, 0, 0));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void GivenUnclustered(params FeedItem[] items) =>
        _itemRepo.GetUnclusteredAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(items);

    private async Task<ClusterRunSummary> Run()
    {
        var handler = new ClusterPendingFeedItemsHandler(_itemRepo, _clusterRepo, _policy, _clock, _unitOfWork);
        Result<ClusterRunSummary> result = await handler.Handle(new ClusterPendingFeedItemsCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    private static FeedItem Item(string title, DateTimeOffset publishedAt, FeedSourceId? source = null) =>
        FeedItem.Create(source ?? FeedSourceId.New(), title, $"https://x.test/{Guid.NewGuid()}", null, publishedAt, null, Now);
}
