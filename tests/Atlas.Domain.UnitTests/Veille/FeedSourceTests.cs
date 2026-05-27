using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class FeedSourceTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_input_succeeds()
    {
        var result = FeedSource.Create(".NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsActive.Should().BeTrue();
        result.Value!.LastPolledAt.Should().BeNull();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/feed")]
    [InlineData("")]
    public void Create_with_invalid_url_fails(string url)
    {
        var result = FeedSource.Create("Source", url, FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.invalid_feed_source");
    }

    [Fact]
    public void Create_with_empty_name_fails()
    {
        var result = FeedSource.Create("   ", "https://example.com/feed", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_with_non_positive_interval_fails()
    {
        var result = FeedSource.Create("Source", "https://example.com/feed", FeedSourceType.Rss, TimeSpan.Zero, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IsDueForPolling_is_true_when_never_polled()
    {
        FeedSource source = CreateSource(TimeSpan.FromMinutes(30));

        source.IsDueForPolling(Now).Should().BeTrue();
    }

    [Fact]
    public void IsDueForPolling_is_false_right_after_polling()
    {
        FeedSource source = CreateSource(TimeSpan.FromMinutes(30));
        source.MarkPolled(Now);

        source.IsDueForPolling(Now.AddMinutes(5)).Should().BeFalse();
    }

    [Fact]
    public void IsDueForPolling_is_true_after_interval_elapsed()
    {
        FeedSource source = CreateSource(TimeSpan.FromMinutes(30));
        source.MarkPolled(Now);

        source.IsDueForPolling(Now.AddMinutes(31)).Should().BeTrue();
    }

    [Fact]
    public void IsDueForPolling_is_false_when_inactive()
    {
        FeedSource source = CreateSource(TimeSpan.FromMinutes(30));
        source.Deactivate();

        source.IsDueForPolling(Now).Should().BeFalse();
    }

    private static FeedSource CreateSource(TimeSpan interval) =>
        FeedSource.Create("Source", "https://example.com/feed", FeedSourceType.Rss, interval, Now).Value!;
}
