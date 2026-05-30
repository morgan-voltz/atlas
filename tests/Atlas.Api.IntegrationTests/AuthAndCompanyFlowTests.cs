using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Atlas.Api.IntegrationTests;

/// <summary>
/// Flux complet de bout en bout sur l'API HTTP réelle : inscription → vérification → connexion →
/// connexion compte INPI → fiche entreprise. Valide le wiring complet (auth JWT, MediatR, EF/Postgres,
/// crypto, adapters INPI simulés par WireMock).
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class AuthAndCompanyFlowTests(AtlasApiFactory factory)
{
    private const string Password = "super-long-password-123";

    private const string CompanyJson = """
    {
      "siren": "552032534",
      "formality": { "diffusionINSEE": "O", "content": { "personneMorale": {
        "identite": { "entreprise": { "denomination": "RENAULT", "formeJuridique": "5710", "codeApe": "2910Z" } },
        "adresseEntreprise": { "adresse": { "codePostal": "92100", "commune": "Boulogne-Billancourt" } } } } }
    }
    """;

    [Fact]
    public async Task Register_verify_login_connect_inpi_then_fetch_company()
    {
        HttpClient client = factory.CreateClient();
        string email = $"e2e-{Guid.NewGuid():N}@example.com";

        // 1. Inscription
        HttpResponseMessage register =
            await client.PostAsJsonAsync("/auth/register", new { email, password = Password });
        register.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Vérification d'email (token capturé par le faux IEmailSender)
        factory.Emails.LastVerificationToken.Should().NotBeNull();
        string token = Uri.EscapeDataString(factory.Emails.LastVerificationToken!);
        HttpResponseMessage verify =
            await client.GetAsync($"/auth/verify-email?userId={factory.Emails.LastUserId}&token={token}");
        verify.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Connexion → access token JWT
        HttpResponseMessage login =
            await client.PostAsJsonAsync("/auth/login", new { email, password = Password });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        string accessToken = (await login.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // 4. Connexion du compte INPI (WireMock simule /sso/login)
        factory.Inpi
            .Given(Request.Create().WithPath("/sso/login").UsingPost())
            .RespondWith(JsonResponse(200, """{"token":"rne-token"}"""));
        HttpResponseMessage connect =
            await client.PostAsJsonAsync("/inpi/connection", new { username = "inpi-user", password = "inpi-pass" });
        connect.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Fiche entreprise par SIREN (WireMock simule /companies/{siren})
        factory.Inpi
            .Given(Request.Create().WithPath("/companies/552032534").UsingGet())
            .RespondWith(JsonResponse(200, CompanyJson));
        HttpResponseMessage company = await client.GetAsync("/companies/552032534");
        company.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await company.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("siren").GetString().Should().Be("552032534");
        body.GetProperty("denomination").GetString().Should().Be("RENAULT");
        body.GetProperty("nafCode").GetString().Should().Be("2910Z");
    }

    [Fact]
    public async Task Protected_endpoint_without_token_is_unauthorized()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/companies/552032534");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Régression F-001 : un userId vide ou mal formé dans la query faisait échouer le binding du
    // Guid (BadHttpRequestException → 500). Une saisie invalide doit produire un 400, jamais un 500.
    [Theory]
    [InlineData("/auth/verify-email?userId=&token=abc")]
    [InlineData("/auth/verify-email?token=abc")]
    [InlineData("/auth/verify-email?userId=not-a-guid&token=abc")]
    [InlineData("/auth/verify-email?userId=11111111-1111-1111-1111-111111111111&token=")]
    public async Task Verify_email_with_invalid_query_returns_bad_request_not_server_error(string url)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static IResponseBuilder JsonResponse(int statusCode, string body) =>
        Response.Create()
            .WithStatusCode(statusCode)
            .WithHeader("Content-Type", "application/json")
            .WithBody(body);
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<AtlasApiFactory>
{
    public const string Name = "api";
}
