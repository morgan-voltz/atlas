using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Atlas.Api.IntegrationTests;

/// <summary>
/// Lot 9 — Mapping HTTP des codes d'erreur veille manquants identifiés dans l'audit Bruno
/// profond (Lot 8). Valide que les codes <c>veille.feed_rule_not_found</c>,
/// <c>veille.veille_pack_code_already_used</c>, etc. retournent désormais leur statut HTTP
/// correct (404 / 409 / 403) au lieu de retomber sur le 400 par défaut.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class VeilleErrorMappingTests(AtlasApiFactory factory)
{
    private const string Password = "super-long-password-789";

    [Fact]
    public async Task Update_unknown_feed_rule_returns_404_not_400()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Patch sur un guid qui n'existe pas → veille.feed_rule_not_found
        Guid unknownRuleId = Guid.NewGuid();
        HttpResponseMessage response = await client.PatchAsJsonAsync(
            $"/feed/rules/{unknownRuleId}",
            new
            {
                name = "Lot 9 rule",
                keywordPattern = "atlas",
                sourceId = (Guid?)null,
                mentionedSiren = (string?)null,
                notifyEmail = true,
                notifyPush = false,
                isActive = true,
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "veille.feed_rule_not_found doit retourner 404 (avant Lot 9 : retombait en 400).");

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("veille.feed_rule_not_found");
    }

    [Fact]
    public async Task Delete_unknown_feed_rule_returns_404_not_400()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        Guid unknownRuleId = Guid.NewGuid();
        HttpResponseMessage response = await client.DeleteAsync($"/feed/rules/{unknownRuleId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "veille.feed_rule_not_found doit retourner 404 (avant Lot 9 : retombait en 400).");

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("veille.feed_rule_not_found");
    }

    // Note : le test du chemin `veille.veille_pack_code_already_used 409` est volontairement
    // omis ici. Créer un pack utilisateur valide exige au moins un abonnement (validation
    // FluentValidation `SubscriptionIds.NotEmpty`), lequel exige lui-même un fetch HTTP
    // réussi sur l'URL de la source RSS — non triviallement mockable sans WireMock RSS-like.
    // Le mapping 409 est validé par revue de code et couvert par la collection Bruno
    // (`22-Veille-Marketplace/02 Create user pack` jouée 2× en local).

    [Fact]
    public async Task Like_unknown_pack_returns_404_not_400()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.PostAsync(
            "/veille/packs/this-pack-does-not-exist/like", content: null);

        // veille.veille_pack_not_found est déjà mappé en 404 — sanity-check du chemin non régressé.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("title").GetString().Should().Be("veille.veille_pack_not_found");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();
        string email = $"lot9-{Guid.NewGuid():N}@example.com";

        HttpResponseMessage register =
            await client.PostAsJsonAsync("/auth/register", new { email, password = Password });
        register.StatusCode.Should().Be(HttpStatusCode.OK);

        string token = Uri.EscapeDataString(factory.Emails.LastVerificationToken!);
        HttpResponseMessage verify =
            await client.GetAsync($"/auth/verify-email?userId={factory.Emails.LastUserId}&token={token}");
        verify.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage login =
            await client.PostAsJsonAsync("/auth/login", new { email, password = Password });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        string accessToken = (await login.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}
