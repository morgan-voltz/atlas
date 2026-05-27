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
    public async Task SearchTrademarks_with_invalid_login_returns_invalid_credentials()
    {
        _server
            .Given(Request.Create().WithPath("/auth/login").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(401));

        Result<PagedResult<TrademarkSummary>> result = await CreateProvider()
            .SearchTrademarksAsync(new TrademarkSearchQuery("nike", 1, 20), Credentials, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.invalid_credentials");
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
