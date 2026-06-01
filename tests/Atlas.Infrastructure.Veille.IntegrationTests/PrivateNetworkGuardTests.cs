using System.Net;
using FluentAssertions;

namespace Atlas.Infrastructure.Veille.IntegrationTests;

/// <summary>
/// Tests de la garde anti-SSRF (audit Lot 1). <see cref="PrivateNetworkGuard.IsBlockedIp"/> alimente
/// le ConnectCallback du client HTTP RSS, qui valide l'IP réellement résolue (rebinding DNS, redirections).
/// </summary>
public sealed class PrivateNetworkGuardTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.169.254")] // métadonnées cloud AWS/GCP
    [InlineData("0.0.0.0")]
    [InlineData("::1")]              // loopback IPv6
    [InlineData("fc00::1")]          // unique-local IPv6 (fc00::/7)
    [InlineData("fd12:3456::1")]     // unique-local IPv6
    [InlineData("::ffff:127.0.0.1")] // IPv4 loopback mappée en IPv6
    [InlineData("::ffff:10.1.2.3")]  // IPv4 privée mappée en IPv6
    public void IsBlockedIp_blocks_internal_addresses(string address)
    {
        PrivateNetworkGuard.IsBlockedIp(IPAddress.Parse(address)).Should().BeTrue();
    }

    [Theory]
    [InlineData("1.1.1.1")]
    [InlineData("8.8.8.8")]
    [InlineData("172.15.0.1")]            // hors 172.16/12
    [InlineData("172.32.0.1")]            // hors 172.16/12
    [InlineData("2001:4860:4860::8888")]  // DNS public IPv6 (Google)
    public void IsBlockedIp_allows_public_addresses(string address)
    {
        PrivateNetworkGuard.IsBlockedIp(IPAddress.Parse(address)).Should().BeFalse();
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("LOCALHOST")]
    [InlineData("127.0.0.1")]
    public void IsBlockedHost_blocks_loopback_literals(string host)
    {
        PrivateNetworkGuard.IsBlockedHost(host).Should().BeTrue();
    }

    [Theory]
    [InlineData("example.com")] // un nom DNS n'est pas résolu ici : validé au ConnectCallback
    [InlineData("8.8.8.8")]
    public void IsBlockedHost_allows_dns_names_and_public_ips(string host)
    {
        PrivateNetworkGuard.IsBlockedHost(host).Should().BeFalse();
    }
}
