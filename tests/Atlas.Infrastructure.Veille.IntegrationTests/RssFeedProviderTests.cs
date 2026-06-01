using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Atlas.Infrastructure.Veille.IntegrationTests;

/// <summary>Valide le parsing/mapping RSS de <see cref="RssFeedProvider"/> contre un serveur HTTP simulé.</summary>
public sealed class RssFeedProviderTests : IDisposable
{
    private const string RssXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <rss version="2.0">
      <channel>
        <title>Test Feed</title>
        <link>https://example.test</link>
        <description>Flux de test</description>
        <item>
          <title>Article 1</title>
          <link>https://example.test/1</link>
          <description>Résumé 1</description>
          <pubDate>Wed, 27 May 2026 10:00:00 GMT</pubDate>
          <category>cat-a</category>
        </item>
        <item>
          <title>Article 2</title>
          <link>https://example.test/2</link>
          <description>Résumé 2</description>
          <pubDate>Tue, 20 May 2026 10:00:00 GMT</pubDate>
        </item>
      </channel>
    </rss>
    """;

    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task FetchAsync_maps_all_items_when_no_since()
    {
        StubFeed("/feed", 200, RssXml);

        Result<IReadOnlyList<FeedItemDraft>> result = await CreateProvider()
            .FetchAsync(SourceFor("/feed"), since: null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value!.Select(item => item.Title).Should().Equal("Article 1", "Article 2");
        result.Value![0].Url.Should().Be("https://example.test/1");
        result.Value![0].Categories.Should().ContainSingle().Which.Should().Be("cat-a");
    }

    [Fact]
    public async Task FetchAsync_filters_items_published_before_since()
    {
        StubFeed("/feed", 200, RssXml);

        Result<IReadOnlyList<FeedItemDraft>> result = await CreateProvider()
            .FetchAsync(SourceFor("/feed"), since: new DateTimeOffset(2026, 5, 25, 0, 0, 0, TimeSpan.Zero), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle().Which.Title.Should().Be("Article 1");
    }

    [Fact]
    public async Task FetchAsync_returns_failure_on_http_error()
    {
        StubFeed("/feed", 404, body: string.Empty);

        Result<IReadOnlyList<FeedItemDraft>> result = await CreateProvider()
            .FetchAsync(SourceFor("/feed"), since: null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.fetch_failed");
    }

    [Fact]
    public async Task FetchAsync_returns_failure_on_non_feed_content()
    {
        StubFeed("/feed", 200, "<html><body>pas un flux</body></html>");

        Result<IReadOnlyList<FeedItemDraft>> result = await CreateProvider()
            .FetchAsync(SourceFor("/feed"), since: null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task FetchAsync_rejects_feed_with_external_entity_dtd_without_resolving_it()
    {
        // Durcissement anti-XXE (audit Lot 1) : un flux portant une DTD avec entité externe doit
        // être rejeté proprement (Result.Fail), sans que l'entité « file:/// » soit résolue.
        const string xxe = """
        <?xml version="1.0" encoding="utf-8"?>
        <!DOCTYPE rss [ <!ENTITY xxe SYSTEM "file:///etc/passwd"> ]>
        <rss version="2.0"><channel><title>&xxe;</title><link>https://x.test</link>
        <description>d</description></channel></rss>
        """;
        StubFeed("/feed", 200, xxe);

        Result<IReadOnlyList<FeedItemDraft>> result = await CreateProvider()
            .FetchAsync(SourceFor("/feed"), since: null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.fetch_failed");
    }

    private static RssFeedProvider CreateProvider() => new(new HttpClient());

    private FeedSource SourceFor(string path) =>
        FeedSource.Create("Test", _server.Url + path, FeedSourceType.Rss, TimeSpan.FromMinutes(30), DateTimeOffset.UtcNow).Value!;

    private void StubFeed(string path, int statusCode, string body) =>
        _server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("Content-Type", "application/rss+xml; charset=utf-8")
                .WithBody(body));
}
