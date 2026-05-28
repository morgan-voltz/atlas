using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Veille.IntegrationTests;

/// <summary>
/// Tests unitaires de la politique d'abonnement aux flux (Lot 2a) :
/// anti-SSRF (rejet IP privées/loopback/link-local/métadonnées cloud) + blocklist d'hôtes.
/// </summary>
public sealed class FeedSubscriptionPolicyTests
{
    private static FeedSubscriptionPolicy NewPolicy(VeilleOptions? options = null) =>
        new(Options.Create(options ?? new VeilleOptions()));

    [Theory]
    [InlineData("http://localhost/feed")]
    [InlineData("http://localhost:8080/x")]
    [InlineData("http://127.0.0.1/feed")]
    [InlineData("http://127.99.99.99/feed")]
    [InlineData("http://10.0.0.1/feed")]
    [InlineData("http://10.255.255.255/feed")]
    [InlineData("http://172.16.0.1/feed")]
    [InlineData("http://172.31.255.255/feed")]
    [InlineData("http://192.168.0.1/feed")]
    [InlineData("http://192.168.255.255/feed")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]  // AWS / GCP metadata
    [InlineData("http://[::1]/feed")]                          // IPv6 loopback
    public void IsUrlAllowed_blocks_loopback_private_and_metadata_ranges(string url)
    {
        NewPolicy().IsUrlAllowed(url).Should().BeFalse();
    }

    [Theory]
    [InlineData("https://flux.example.com/rss")]
    [InlineData("https://www.cnil.fr/fr/rss.xml")]
    [InlineData("http://1.1.1.1/feed")]            // IPv4 publique
    [InlineData("http://172.15.0.1/feed")]         // hors 172.16/12
    [InlineData("http://172.32.0.1/feed")]         // hors 172.16/12
    public void IsUrlAllowed_accepts_public_hosts(string url)
    {
        NewPolicy().IsUrlAllowed(url).Should().BeTrue();
    }

    [Fact]
    public void IsUrlAllowed_respects_BlockedHostFragments()
    {
        var opts = new VeilleOptions { BlockedHostFragments = new List<string> { "spam.example" } };

        NewPolicy(opts).IsUrlAllowed("https://news.spam.example.com/rss").Should().BeFalse();
        NewPolicy(opts).IsUrlAllowed("https://news.legitimate.com/rss").Should().BeTrue();
    }

    [Fact]
    public void IsUrlAllowed_returns_true_for_unparsable_url()
    {
        // L'URL non-absolue est rejetée plus loin par FeedSource.Create (erreur dédiée).
        NewPolicy().IsUrlAllowed("pas-une-url").Should().BeTrue();
    }

    [Fact]
    public void MaxSubscriptionsPerUser_returns_null_when_non_positive()
    {
        NewPolicy(new VeilleOptions { MaxSubscriptionsPerUser = 0 }).MaxSubscriptionsPerUser.Should().BeNull();
        NewPolicy(new VeilleOptions { MaxSubscriptionsPerUser = -1 }).MaxSubscriptionsPerUser.Should().BeNull();
    }

    [Fact]
    public void MaxSubscriptionsPerUser_returns_value_when_positive()
    {
        NewPolicy(new VeilleOptions { MaxSubscriptionsPerUser = 50 }).MaxSubscriptionsPerUser.Should().Be(50);
    }
}
