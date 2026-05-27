using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Infrastructure.Inpi.Authentication;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Inpi.Rne;
using Atlas.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Atlas.Infrastructure.Inpi.IntegrationTests;

/// <summary>
/// Valide les adapters INPI (auth RNE + lecture entreprise) contre un serveur HTTP simulé (WireMock).
/// ⚠️ Les payloads reproduisent la structure RNE *supposée* : ces tests valident le pipeline HTTP
/// (auth, cache de token, retry 401) et le parsing/mapping ; la conformité au schéma RNE réel reste
/// à confirmer avec un compte INPI.
/// </summary>
public sealed class RneAdaptersIntegrationTests : IDisposable
{
    private static readonly InpiAccessCredentials Credentials = new("user@inpi.fr", "secret");
    private const string Siren = "552032534";

    private const string CompanyJson = """
    {
      "siren": "552032534",
      "formality": {
        "content": {
          "personneMorale": {
            "identite": {
              "denomination": "RENAULT",
              "formeJuridique": "5710",
              "dateImmatriculation": "1990-01-15"
            },
            "entreprise": {
              "activitePrincipale": { "codeNAF": "2910Z" },
              "adresseEntreprise": {
                "numVoie": "122", "typeVoie": "AV", "voie": "du General Leclerc",
                "codePostal": "92100", "commune": "Boulogne-Billancourt", "pays": "France"
              }
            },
            "composition": {
              "pouvoirs": [
                { "individu": { "descriptionPersonne": { "nom": "Dupont" } }, "roleEntreprise": "Président" }
              ]
            },
            "indicateurDiffusionINSEE": "O"
          }
        }
      }
    }
    """;

    private const string SearchJson = """
    [
      { "siren": "552032534", "formality": { "content": { "personneMorale": {
          "identite": { "denomination": "RENAULT" },
          "entreprise": { "activitePrincipale": { "codeNAF": "2910Z" }, "adresseEntreprise": { "commune": "Boulogne-Billancourt" } } } } } },
      { "siren": "775665011", "formality": { "content": { "personneMorale": {
          "identite": { "denomination": "RENAULT TRUCKS" },
          "entreprise": { "adresseEntreprise": { "commune": "Saint-Priest" } } } } } }
    ]
    """;

    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task Authenticate_with_valid_credentials_returns_session()
    {
        StubLogin();

        Result<InpiSession> result = await CreateAuthProvider().AuthenticateAsync("u", "p", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("test-rne-token");
    }

    [Fact]
    public async Task Authenticate_with_unauthorized_returns_invalid_credentials()
    {
        StubLogin(statusCode: 401);

        Result<InpiSession> result = await CreateAuthProvider().AuthenticateAsync("u", "p", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.invalid_credentials");
    }

    [Fact]
    public async Task Authenticate_with_server_error_returns_unavailable()
    {
        StubLogin(statusCode: 500);

        Result<InpiSession> result = await CreateAuthProvider().AuthenticateAsync("u", "p", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.unavailable");
    }

    [Fact]
    public async Task GetBySiren_maps_the_company_sheet()
    {
        StubLogin();
        StubFiche(Siren, 200, CompanyJson);

        Result<UniteLegale> result = await CreateCompanyProvider()
            .GetBySirenAsync(ParseSiren(Siren), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        UniteLegale company = result.Value!;
        company.Denomination.Should().Be("RENAULT");
        company.FormeJuridique.Should().Be("5710");
        company.ActivitePrincipale!.Value.Code.Should().Be("2910Z");
        company.Adresse!.City.Should().Be("Boulogne-Billancourt");
        company.Adresse!.Line.Should().Be("122 AV du General Leclerc");
        company.DateCreation.Should().Be(new DateOnly(1990, 1, 15));
        company.IsDiffusible.Should().BeTrue();
        company.Dirigeants.Should().ContainSingle(dirigeant => dirigeant.Nom == "Dupont");
    }

    [Fact]
    public async Task GetBySiren_returns_not_found_on_404()
    {
        StubLogin();
        StubFiche(Siren, 404, body: string.Empty);

        Result<UniteLegale> result = await CreateCompanyProvider()
            .GetBySirenAsync(ParseSiren(Siren), Credentials, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.not_found");
    }

    [Fact]
    public async Task GetBySiren_caches_the_token_across_calls()
    {
        StubLogin();
        StubFiche(Siren, 200, CompanyJson);
        RneCompanyProvider provider = CreateCompanyProvider();

        await provider.GetBySirenAsync(ParseSiren(Siren), Credentials, CancellationToken.None);
        await provider.GetBySirenAsync(ParseSiren(Siren), Credentials, CancellationToken.None);

        LoginCallCount().Should().Be(1);
    }

    [Fact]
    public async Task GetBySiren_re_authenticates_once_on_401()
    {
        StubLogin();
        StubFiche(Siren, 401, body: string.Empty);

        Result<UniteLegale> result = await CreateCompanyProvider()
            .GetBySirenAsync(ParseSiren(Siren), Credentials, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.unavailable");
        LoginCallCount().Should().Be(2);
    }

    [Fact]
    public async Task SearchByName_maps_the_result_page()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/companies").UsingGet())
            .RespondWith(JsonResponse(200, SearchJson));

        Result<PagedResult<CompanySummary>> result = await CreateCompanyProvider()
            .SearchByNameAsync(new CompanySearchQuery("renault", 1, 20), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items[0].Siren.Value.Should().Be("552032534");
        result.Value!.Items[0].Ville.Should().Be("Boulogne-Billancourt");
        result.Value!.Items[1].Denomination.Should().Be("RENAULT TRUCKS");
    }

    private static Siren ParseSiren(string value) => Atlas.Domain.Companies.Siren.Create(value).Value!;

    private HttpClient CreateHttpClient() => new() { BaseAddress = new Uri(_server.Url! + "/") };

    private InpiAuthenticationProvider CreateAuthProvider() =>
        new(CreateHttpClient(), Options.Create(new InpiOptions()));

    private RneCompanyProvider CreateCompanyProvider() =>
        new(CreateHttpClient(), CreateAuthProvider(), new MemoryCache(new MemoryCacheOptions()));

    private void StubLogin(int statusCode = 200) =>
        _server
            .Given(Request.Create().WithPath("/sso/login").UsingPost())
            .RespondWith(JsonResponse(statusCode, """{"token":"test-rne-token"}"""));

    private void StubFiche(string siren, int statusCode, string body) =>
        _server
            .Given(Request.Create().WithPath($"/companies/{siren}").UsingGet())
            .RespondWith(JsonResponse(statusCode, body));

    private static IResponseBuilder JsonResponse(int statusCode, string body) =>
        Response.Create()
            .WithStatusCode(statusCode)
            .WithHeader("Content-Type", "application/json")
            .WithBody(body);

    private int LoginCallCount() =>
        _server.LogEntries.Count(entry => entry.RequestMessage?.Path == "/sso/login");
}
