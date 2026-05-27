using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Atlas.Domain.Inpi;
using Atlas.Infrastructure.Inpi.Common;
using Atlas.Shared.Result;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Atlas.Infrastructure.Inpi.Authentication;

/// <summary>
/// Authentification RNE via <c>POST /sso/login</c>. Les identifiants ne sont JAMAIS loggés.
/// </summary>
internal sealed class InpiAuthenticationProvider(HttpClient httpClient, IOptions<InpiOptions> options)
    : IInpiAuthenticationProvider
{
    public async Task<Result<InpiSession>> AuthenticateAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("sso/login", new LoginRequest(username, password), ct);
        }
        catch (HttpRequestException)
        {
            return Result<InpiSession>.Fail(InpiErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Result<InpiSession>.Fail(InpiErrors.Unavailable);
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest)
        {
            return Result<InpiSession>.Fail(InpiErrors.InvalidCredentials);
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result<InpiSession>.Fail(InpiErrors.Unavailable);
        }

        LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        if (body is null || string.IsNullOrWhiteSpace(body.Token))
        {
            return Result<InpiSession>.Fail(InpiErrors.Unavailable);
        }

        DateTimeOffset expiresAt = ReadExpiry(body.Token, options.Value.TokenLifetimeFallback);
        return Result<InpiSession>.Ok(new InpiSession(body.Token, expiresAt));
    }

    private static DateTimeOffset ReadExpiry(string jwt, TimeSpan fallback)
    {
        try
        {
            var token = new JsonWebToken(jwt);
            return token.ValidTo == DateTime.MinValue
                ? DateTimeOffset.UtcNow.Add(fallback)
                : new DateTimeOffset(token.ValidTo, TimeSpan.Zero);
        }
        catch (ArgumentException)
        {
            return DateTimeOffset.UtcNow.Add(fallback);
        }
    }

    private sealed record LoginRequest(
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("password")] string Password);

    private sealed record LoginResponse(
        [property: JsonPropertyName("token")] string? Token);
}
