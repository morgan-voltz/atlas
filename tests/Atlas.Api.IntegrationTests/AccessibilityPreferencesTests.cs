using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Atlas.Api.IntegrationTests;

/// <summary>
/// Bout en bout sur les endpoints <c>/user/preferences/accessibility</c> (cf. <c>docs/06</c> §6.3).
/// Valide GET (défauts), PUT (mutation), et la persistance multi-requête (round-trip).
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class AccessibilityPreferencesTests(AtlasApiFactory factory)
{
    private const string Password = "super-long-password-456";

    [Fact]
    public async Task Get_returns_default_preferences_for_new_user()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync("/user/preferences/accessibility");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("highContrast").GetBoolean().Should().BeFalse();
        body.GetProperty("reduceMotion").GetBoolean().Should().BeFalse();
        body.GetProperty("fontPreference").GetString().Should().Be("Default");
    }

    [Fact]
    public async Task Put_then_get_round_trips_preferences()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage put = await client.PutAsJsonAsync(
            "/user/preferences/accessibility",
            new { highContrast = true, reduceMotion = true, fontPreference = "DyslexiaFriendly" });
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage get = await client.GetAsync("/user/preferences/accessibility");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await get.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("highContrast").GetBoolean().Should().BeTrue();
        body.GetProperty("reduceMotion").GetBoolean().Should().BeTrue();
        body.GetProperty("fontPreference").GetString().Should().Be("DyslexiaFriendly");
    }

    [Fact]
    public async Task Unauthenticated_access_is_unauthorized()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/user/preferences/accessibility");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();
        string email = $"a11y-{Guid.NewGuid():N}@example.com";

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
