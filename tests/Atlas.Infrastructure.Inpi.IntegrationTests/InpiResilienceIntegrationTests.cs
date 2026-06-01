using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Infrastructure.Inpi;
using Atlas.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Atlas.Infrastructure.Inpi.IntegrationTests;

/// <summary>
/// Valide la résilience HTTP câblée via la DI (audit Lot 2) : un appel RNE qui reçoit une erreur
/// transitoire (503) est rejoué automatiquement par le pipeline standard, puis aboutit au rétablissement.
/// Contrairement aux autres tests, on passe par <see cref="DependencyInjection.AddInpiInfrastructure"/>
/// pour exercer le handler de résilience réel (les providers construits « à la main » ne l'ont pas).
/// </summary>
public sealed class InpiResilienceIntegrationTests : IDisposable
{
    private const string Siren = "552032534";

    private const string CompanyJson = """
    {
      "siren": "552032534",
      "formality": { "content": { "personneMorale": {
        "identite": { "denomination": "RENAULT" }
      } } }
    }
    """;

    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task RneClient_retries_transient_503_then_recovers()
    {
        const string scenario = "fiche-transient";
        StubLogin();

        // 1re réponse : 503 (transitoire) → le pipeline de résilience doit réessayer.
        _server
            .Given(Request.Create().WithPath($"/companies/{Siren}").UsingGet())
            .InScenario(scenario)
            .WillSetStateTo("recovered")
            .RespondWith(Response.Create().WithStatusCode(503));

        // 2e réponse (après transition d'état) : 200 OK.
        _server
            .Given(Request.Create().WithPath($"/companies/{Siren}").UsingGet())
            .InScenario(scenario)
            .WhenStateIs("recovered")
            .RespondWith(JsonResponse(200, CompanyJson));

        ICompanyDataProvider provider = BuildProvider();

        Result<UniteLegale> result = await provider.GetBySirenAsync(
            ParseSiren(Siren), new InpiAccessCredentials("u", "p"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue("le 503 transitoire doit être réessayé puis aboutir au 200");
        result.Value!.Denomination.Should().Be("RENAULT");
        FicheCallCount().Should().Be(2, "un retry exactement : 503 puis 200");
    }

    private ICompanyDataProvider BuildProvider()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Inpi:RneBaseUrl"] = _server.Url! + "/",
                ["Inpi:PiBaseUrl"] = _server.Url! + "/",
                // Timeout court : attempt 5 s, total 20 s, sampling 10 s — garde le test rapide.
                ["Inpi:TimeoutSeconds"] = "5",
            })
            .Build();

        ServiceProvider services = new ServiceCollection()
            .AddInpiInfrastructure(configuration)
            .BuildServiceProvider();

        return services.GetRequiredService<ICompanyDataProvider>();
    }

    private static Siren ParseSiren(string value) => Atlas.Domain.Companies.Siren.Create(value).Value!;

    private void StubLogin() =>
        _server
            .Given(Request.Create().WithPath("/sso/login").UsingPost())
            .RespondWith(JsonResponse(200, """{"token":"test-rne-token"}"""));

    private int FicheCallCount() =>
        _server.LogEntries.Count(entry => entry.RequestMessage?.Path == $"/companies/{Siren}");

    private static IResponseBuilder JsonResponse(int statusCode, string body) =>
        Response.Create()
            .WithStatusCode(statusCode)
            .WithHeader("Content-Type", "application/json")
            .WithBody(body);
}
