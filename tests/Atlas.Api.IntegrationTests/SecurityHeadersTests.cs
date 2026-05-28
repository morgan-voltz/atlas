using System.Net.Http;
using FluentAssertions;

namespace Atlas.Api.IntegrationTests;

/// <summary>
/// Vérifie que tous les en-têtes de sécurité du Lot 2b sont présents sur les réponses HTTP.
/// </summary>
public sealed class SecurityHeadersTests(AtlasApiFactory factory) : IClassFixture<AtlasApiFactory>
{
    private readonly AtlasApiFactory _factory = factory;

    [Theory]
    [InlineData("/")]
    [InlineData("/auth/login")]    // 400 ou 405 attendu mais les headers doivent être là.
    public async Task Response_contains_all_security_headers(string path)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(path);

        response.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.GetValues("Referrer-Policy").Should().ContainSingle().Which.Should().Be("strict-origin-when-cross-origin");
        response.Headers.GetValues("X-Frame-Options").Should().ContainSingle().Which.Should().Be("DENY");
        response.Headers.GetValues("Content-Security-Policy").Should().ContainSingle()
            .Which.Should().Contain("default-src 'none'").And.Contain("frame-ancestors 'none'");
        response.Headers.GetValues("Permissions-Policy").Should().ContainSingle()
            .Which.Should().Contain("camera=()").And.Contain("geolocation=()");
    }
}
