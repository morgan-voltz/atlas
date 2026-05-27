using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class FeedItemTests
{
    private static readonly FeedSourceId SourceId = FeedSourceId.New();
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ComputeHash_is_stable_for_same_url_and_title()
    {
        FeedItem.ComputeHash("https://x.test/1", "Titre")
            .Should().Be(FeedItem.ComputeHash("https://x.test/1", "Titre"));
    }

    [Fact]
    public void ComputeHash_differs_when_url_or_title_differ()
    {
        string baseline = FeedItem.ComputeHash("https://x.test/1", "Titre");

        FeedItem.ComputeHash("https://x.test/2", "Titre").Should().NotBe(baseline);
        FeedItem.ComputeHash("https://x.test/1", "Autre").Should().NotBe(baseline);
    }

    [Fact]
    public void Create_sets_hash_consistent_with_ComputeHash()
    {
        FeedItem item = FeedItem.Create(SourceId, "Titre", "https://x.test/1", "résumé", Now, ["a", "b"], Now);

        item.ContentHash.Should().Be(FeedItem.ComputeHash("https://x.test/1", "Titre"));
        item.Categories.Should().Be("a\nb");
        item.SourceId.Should().Be(SourceId);
    }

    [Fact]
    public void Create_truncates_overly_long_title()
    {
        string longTitle = new('x', FeedItem.MaxTitleLength + 50);

        FeedItem item = FeedItem.Create(SourceId, longTitle, null, null, Now, null, Now);

        item.Title.Length.Should().Be(FeedItem.MaxTitleLength);
    }

    [Fact]
    public void Create_falls_back_to_placeholder_for_empty_title()
    {
        FeedItem item = FeedItem.Create(SourceId, "   ", null, null, Now, null, Now);

        item.Title.Should().Be("(sans titre)");
    }
}
