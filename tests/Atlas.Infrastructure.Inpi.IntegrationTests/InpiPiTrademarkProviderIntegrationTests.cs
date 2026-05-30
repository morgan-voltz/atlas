using System.Text.Json;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Infrastructure.Inpi.Pi;
using Atlas.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Atlas.Infrastructure.Inpi.IntegrationTests;

/// <summary>
/// Valide l'adapter INPI PI (recherche marques) contre WireMock. ⚠️ Le flux d'auth PI (login → cookies
/// access_token + XSRF) et le contrat de recherche sont *supposés* ; à confirmer avec l'API PI réelle.
/// </summary>
public sealed class InpiPiTrademarkProviderIntegrationTests : IDisposable
{
    private static readonly InpiAccessCredentials Credentials = new("user@inpi.fr", "secret");

    private const string SearchJson = """
    {
      "results": [
        { "marque": "NIKE", "deposant": "Nike Inc.", "numeroDepot": "4001234", "dateDepot": "2018-03-15", "statut": "Enregistrée" },
        { "marque": "NIKE AIR", "deposant": "Nike Inc.", "numeroDepot": "4005678", "dateDepot": "2019-06-01", "statut": "Enregistrée" }
      ],
      "total": 2
    }
    """;

    private const string NoticeJson = """
    {
      "marque": "NIKE",
      "deposant": "Nike Inc.",
      "numeroDepot": "4001234",
      "dateDepot": "2018-03-15",
      "dateEnregistrement": "2018-09-01",
      "statut": "Enregistrée",
      "type": "verbale",
      "hasImage": true,
      "classes": [ { "numero": 25, "libelle": "Vêtements" }, { "numero": 35, "libelle": "Publicité" } ]
    }
    """;

    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task SearchTrademarks_authenticates_then_maps_results()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/search").UsingPost())
            .RespondWith(JsonResponse(200, SearchJson));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("nike", 1, 20), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items[0].Denomination.Should().Be("NIKE");
        result.Value!.Items[0].DepositNumber.Value.Should().Be("4001234");
        result.Value!.Items[0].DateDepot.Should().Be(new DateOnly(2018, 3, 15));
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchTrademarks_returns_empty_page_on_204_no_content()
    {
        // L'INPI renvoie 204 No Content quand la recherche n'a aucun résultat : page vide,
        // pas une erreur. Régression : ce 204 faisait lever une JsonException → 502.
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/search").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(204));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("zzzznomatch", 1, 20), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchPatents_returns_empty_page_on_204_no_content()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/brevets/search").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(204));

        Result<PagedResult<PatentSummary>> result = await CreateProvider()
            .SearchPatentsAsync(
                new PatentSearchQuery(null, null, "zzzznomatch", 1, 20), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchTrademarks_fails_unavailable_when_csrf_primer_returns_no_cookie()
    {
        // Si le primer CSRF échoue sans poser de cookie XSRF-TOKEN (réseau cassé, INPI
        // indisponible…), le login ne peut pas continuer : on retourne inpi.unavailable.
        _server
            .Given(Request.Create().WithPath("/auth/login").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(503));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("nike", 1, 20), Credentials, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.unavailable");
    }

    [Fact]
    public async Task GetTrademark_authenticates_then_maps_the_notice()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/notice/4001234").UsingGet())
            .RespondWith(JsonResponse(200, NoticeJson));

        Result<TrademarkDetail> result = await CreateProvider()
            .GetTrademarkAsync(new DepositNumber("4001234"), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Denomination.Should().Be("NIKE");
        result.Value!.Type.Should().Be("verbale");
        result.Value!.HasImage.Should().BeTrue();
        result.Value!.ClassesNice.Should().HaveCount(2);
        result.Value!.ClassesNice[0].Number.Should().Be(25);
        result.Value!.DateEnregistrement.Should().Be(new DateOnly(2018, 9, 1));
    }

    [Fact]
    public async Task GetTrademark_returns_not_found_on_404()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/notice/0000000").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        Result<TrademarkDetail> result = await CreateProvider()
            .GetTrademarkAsync(new DepositNumber("0000000"), Credentials, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("trademarks.not_found");
    }

    /// <summary>
    /// Lot 11 — Valide que l'adapter émet bien le primer CSRF (<c>X-CSRF-TOKEN: Fetch</c>)
    /// avant le vrai POST de login (qui doit, lui, porter <c>X-XSRF-TOKEN: &lt;token&gt;</c>
    /// + le cookie <c>XSRF-TOKEN=&lt;token&gt;</c> côté pattern double-submit).
    /// Régression contre l'INPI réel détectée le 29 mai 2026.
    /// </summary>
    [Fact]
    public async Task SearchTrademarks_performs_csrf_primer_before_login()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/search").UsingPost())
            .RespondWith(JsonResponse(200, SearchJson));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("nike", 1, 20), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        // Inspecte les requêtes reçues par WireMock sur /auth/login.
        List<WireMock.Logging.ILogEntry> loginRequests = _server.LogEntries
            .Where(entry => entry.RequestMessage?.AbsolutePath?.EndsWith("/auth/login", StringComparison.Ordinal) == true)
            .ToList();

        loginRequests.Should().HaveCount(2, "l'adapter doit émettre un primer puis le vrai login.");

        // Requête 1 : primer CSRF.
        WireMock.IRequestMessage primer = loginRequests[0].RequestMessage!;
        primer.Headers!.Should().ContainKey("X-CSRF-TOKEN")
            .WhoseValue.Should().BeEquivalentTo(new[] { "Fetch" });
        primer.Body.Should().BeNullOrEmpty("le primer CSRF n'envoie pas de body, seul le header compte.");

        // Requête 2 : vrai login. Doit porter X-XSRF-TOKEN + Cookie XSRF-TOKEN issus du primer.
        WireMock.IRequestMessage login = loginRequests[1].RequestMessage!;
        login.Headers!.Should().ContainKey("X-XSRF-TOKEN");
        login.Headers!["X-XSRF-TOKEN"].Single().Should().NotBeNullOrWhiteSpace();
        login.Cookies!.Should().ContainKey("XSRF-TOKEN");
        login.Body.Should().Contain("user@inpi.fr");
    }

    [Fact]
    public async Task SearchTrademarks_fails_invalid_credentials_when_primer_succeeds_but_real_login_returns_401()
    {
        // Distinct from `SearchTrademarks_with_invalid_login_returns_invalid_credentials` : ici
        // le primer réussit (cookie posé) et c'est la 2e requête (le vrai login) qui rejette.
        // Reproduit le scénario où le compte API n'a pas accès au catalogue PI : INPI répond
        // 401 « Invalid credentials » après que le CSRF soit validé.
        _server
            .Given(Request.Create().WithPath("/auth/login").UsingPost()
                .WithHeader("X-CSRF-TOKEN", "Fetch"))
            .RespondWith(Response.Create()
                .WithStatusCode(403)
                .WithHeader("Set-Cookie", "XSRF-TOKEN=primer-xsrf; Path=/"));

        _server
            .Given(Request.Create().WithPath("/auth/login").UsingPost()
                .WithHeader("X-XSRF-TOKEN", "primer-xsrf"))
            .RespondWith(Response.Create().WithStatusCode(401));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("nike", 1, 20), Credentials, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.invalid_credentials");
    }

    /// <summary>
    /// Lot 13 — Valide que le body POST <c>/marques/search</c> respecte le contrat
    /// <c>TrademarkQuery</c> de la spec INPI v2 (<c>docs/INPI/APIDiffusionV2.json</c>) :
    /// <c>collections</c> obligatoire (sans quoi le backend INPI retourne 500 « SolR no body »),
    /// <c>query</c> au format SolR INPI (<c>[Mark=...]</c>) et non le terme brut, pagination
    /// par <c>position</c> 0-based et <c>size</c>.
    /// </summary>
    [Fact]
    public async Task SearchTrademarks_body_matches_v2_TrademarkQuery_contract()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/search").UsingPost())
            .RespondWith(JsonResponse(200, SearchJson));

        // Page 2 + pageSize 10 → position = 10.
        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("danone", 2, 10), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        WireMock.IRequestMessage searchRequest = _server.LogEntries
            .Where(e => e.RequestMessage?.AbsolutePath?.EndsWith("/marques/search", StringComparison.Ordinal) == true)
            .Select(e => e.RequestMessage!)
            .Single();

        searchRequest.Body.Should().NotBeNullOrWhiteSpace();
        JsonDocument body = JsonDocument.Parse(searchRequest.Body!);

        body.RootElement.GetProperty("query").GetString().Should().Be("[Mark=danone]",
            "le terme libre est wrappé en clause SolR sur le champ Mark.");
        body.RootElement.GetProperty("position").GetInt32().Should().Be(10,
            "position = (page-1) * pageSize, soit (2-1)*10 = 10 en 0-based.");
        body.RootElement.GetProperty("size").GetInt32().Should().Be(10);

        JsonElement collections = body.RootElement.GetProperty("collections");
        collections.ValueKind.Should().Be(JsonValueKind.Array);
        var values = collections.EnumerateArray().Select(e => e.GetString()).ToList();
        values.Should().BeEquivalentTo(["FR", "EU", "WO"],
            "collections conformes à la spec INPI v2 (TrademarkQuery : FR + EU/EUIPO + WO/OMPI).");

        // Header Accept doit être application/json pour ne pas recevoir du XML.
        searchRequest.Headers!.Should().ContainKey("Accept");
        searchRequest.Headers!["Accept"].Single().Should().Be("application/json");
    }

    /// <summary>
    /// Lot 13 — Valide le builder SolR pour brevets : critères title / applicant / inventor
    /// joints par AND avec les champs INPI corrects (<c>TIT</c>, <c>DEPOSANT</c>, <c>INV</c>),
    /// collections par défaut <c>FR/EP/WO/CCP</c>, position 0-based.
    /// </summary>
    [Fact]
    public async Task SearchPatents_body_matches_v2_PatentQuery_contract()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/brevets/search").UsingPost())
            .RespondWith(JsonResponse(200, "{\"results\":[],\"total\":0}"));

        // PatentSearchQuery(Title, Inventor, Applicant, Page, PageSize)
        Result<PagedResult<PatentSummary>> result = await CreateProvider()
            .SearchPatentsAsync(
                new PatentSearchQuery("electric battery", Inventor: null, Applicant: "RENAULT", 1, 20),
                Credentials,
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        WireMock.IRequestMessage searchRequest = _server.LogEntries
            .Where(e => e.RequestMessage?.AbsolutePath?.EndsWith("/brevets/search", StringComparison.Ordinal) == true)
            .Select(e => e.RequestMessage!)
            .Single();

        searchRequest.Body.Should().NotBeNullOrWhiteSpace();
        JsonDocument body = JsonDocument.Parse(searchRequest.Body!);

        // Title + Applicant fournis, Inventor null → 2 clauses jointes par AND.
        body.RootElement.GetProperty("query").GetString()
            .Should().Be("[TIT=electric battery] AND [DEPOSANT=RENAULT]");
        body.RootElement.GetProperty("position").GetInt32().Should().Be(0);
        body.RootElement.GetProperty("size").GetInt32().Should().Be(20);

        var collections = body.RootElement.GetProperty("collections")
            .EnumerateArray().Select(e => e.GetString()).ToList();
        collections.Should().BeEquivalentTo(["FR", "EP", "WO", "CCP"]);
    }

    /// <summary>
    /// Lot 13 — Cas dégénéré brevets : aucun critère → fallback <c>[TIT=*]</c> pour ne pas
    /// envoyer une requête vide qui serait 500 côté INPI.
    /// </summary>
    [Fact]
    public async Task SearchPatents_with_no_criteria_falls_back_to_TIT_wildcard()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/brevets/search").UsingPost())
            .RespondWith(JsonResponse(200, "{\"results\":[],\"total\":0}"));

        await CreateProvider()
            .SearchPatentsAsync(
                new PatentSearchQuery(null, null, null, 1, 20),
                Credentials,
                CancellationToken.None);

        WireMock.IRequestMessage searchRequest = _server.LogEntries
            .Where(e => e.RequestMessage?.AbsolutePath?.EndsWith("/brevets/search", StringComparison.Ordinal) == true)
            .Select(e => e.RequestMessage!)
            .Single();

        JsonDocument body = JsonDocument.Parse(searchRequest.Body!);
        body.RootElement.GetProperty("query").GetString().Should().Be("[TIT=*]");
    }

    /// <summary>
    /// Lot 12 — Valide que les requêtes post-login envoient le **cookie** `XSRF-TOKEN` en plus
    /// de `access_token`, ET le header `X-XSRF-TOKEN`. Le serveur Spring Security applique le
    /// pattern double-submit cookie : il compare le header `X-XSRF-TOKEN` à la valeur du cookie
    /// `XSRF-TOKEN` et rejette en 403 « Could not verify the provided CSRF token because your
    /// session was not found. » sans cette double présence. Régression INPI réelle détectée le
    /// 2026-05-29 post-Lot 11.
    /// </summary>
    [Fact]
    public async Task SearchTrademarks_sends_both_access_token_and_xsrf_token_cookies()
    {
        StubLogin();
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/search").UsingPost())
            .RespondWith(JsonResponse(200, SearchJson));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("nike", 1, 20), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        WireMock.IRequestMessage searchRequest = _server.LogEntries
            .Where(e => e.RequestMessage?.AbsolutePath?.EndsWith("/marques/search", StringComparison.Ordinal) == true)
            .Select(e => e.RequestMessage!)
            .Single();

        // Double-submit côté requête : cookie XSRF-TOKEN + header X-XSRF-TOKEN.
        searchRequest.Cookies!.Should().ContainKey("XSRF-TOKEN");
        searchRequest.Cookies!.Should().ContainKey("access_token");
        searchRequest.Headers!.Should().ContainKey("X-XSRF-TOKEN");

        // Sanity : les deux valeurs XSRF (cookie et header) doivent être identiques.
        string cookieXsrf = searchRequest.Cookies!["XSRF-TOKEN"];
        string headerXsrf = searchRequest.Headers!["X-XSRF-TOKEN"].Single();
        cookieXsrf.Should().Be(headerXsrf, "le serveur compare la valeur du header à celle du cookie.");
    }

    [Fact]
    public async Task GetTrademarkImage_returns_bytes_with_content_type()
    {
        StubLogin();
        byte[] png = [0x89, 0x50, 0x4E, 0x47];
        _server
            .Given(Request.Create().WithPath("/services/apidiffusion/api/marques/image/4001234").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithHeader("Content-Type", "image/png").WithBody(png));

        Result<TrademarkImage> result = await CreateProvider()
            .GetTrademarkImageAsync(new DepositNumber("4001234"), Credentials, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ContentType.Should().Be("image/png");
        result.Value!.Content.Should().Equal(png);
    }

    private InpiPiTrademarkProvider CreateProvider()
    {
        var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false })
        {
            BaseAddress = new Uri(_server.Url! + "/"),
        };
        var options = Options.Create(new InpiOptions());
        return new InpiPiTrademarkProvider(httpClient, new MemoryCache(new MemoryCacheOptions()), options);
    }

    private void StubLogin() =>
        _server
            .Given(Request.Create().WithPath("/auth/login").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Set-Cookie", "access_token=abc-token; Path=/; HttpOnly", "XSRF-TOKEN=xsrf-123; Path=/"));

    private static IResponseBuilder JsonResponse(int statusCode, string body) =>
        Response.Create()
            .WithStatusCode(statusCode)
            .WithHeader("Content-Type", "application/json")
            .WithBody(body);
}
