using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class FeedItemClusterTests
{
    private static readonly FeedSourceId SourceA = FeedSourceId.New();
    private static readonly FeedSourceId SourceB = FeedSourceId.New();
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_seeds_count_and_bounds_from_the_seed_item()
    {
        FeedItem seed = Item(SourceA, Now.AddHours(-2));

        FeedItemCluster cluster = FeedItemCluster.Create(simHash: 42L, seed, Now);

        cluster.SimHash.Should().Be(42L);
        cluster.ItemCount.Should().Be(1);
        cluster.FirstPublishedAt.Should().Be(seed.PublishedAt);
        cluster.LastPublishedAt.Should().Be(seed.PublishedAt);
        cluster.CreatedAt.Should().Be(Now);
    }

    [Fact]
    public void AddItem_increments_count_and_extends_bounds()
    {
        FeedItem seed = Item(SourceA, Now.AddHours(-2));
        FeedItemCluster cluster = FeedItemCluster.Create(42L, seed, Now);

        cluster.AddItem(Item(SourceB, Now.AddHours(-5)));
        cluster.AddItem(Item(SourceB, Now.AddHours(1)));

        cluster.ItemCount.Should().Be(3);
        cluster.FirstPublishedAt.Should().Be(Now.AddHours(-5));
        cluster.LastPublishedAt.Should().Be(Now.AddHours(1));
    }

    private static FeedItem Item(FeedSourceId sourceId, DateTimeOffset publishedAt) =>
        FeedItem.Create(sourceId, "Titre", "https://x.test/1", "résumé", publishedAt, null, Now);
}
