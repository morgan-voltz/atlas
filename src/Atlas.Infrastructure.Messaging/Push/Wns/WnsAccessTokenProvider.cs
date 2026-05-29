using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Push.Wns;

/// <summary>
/// Échange <c>(client_id=PackageSid, client_secret=ClientSecret, scope=notify.windows.com)</c>
/// contre un access token Bearer WNS via <c>POST login.live.com/accesstoken.srf</c>.
/// Le token est mis en cache jusqu'à 5 min avant son expiration (TTL Microsoft = ~24 h).
/// Refresh thread-safe.
/// </summary>
internal sealed class WnsAccessTokenProvider(
    HttpClient httpClient,
    IOptions<WnsOptions> options) : IWnsAccessTokenProvider, IDisposable
{
    internal const string TokenEndpoint = "https://login.live.com/accesstoken.srf";
    private const string Scope = "notify.windows.com";
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly WnsOptions _options = options.Value;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _cachedExpiresAt;

    public void Dispose() => _refreshLock.Dispose();

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow + RefreshMargin < _cachedExpiresAt)
        {
            return _cachedToken;
        }

        await _refreshLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow + RefreshMargin < _cachedExpiresAt)
            {
                return _cachedToken;
            }

            EnsureConfigured();

            DateTimeOffset issuedAt = DateTimeOffset.UtcNow;
            (string token, int expiresInSeconds) = await ExchangeAsync(ct);

            _cachedToken = token;
            _cachedExpiresAt = issuedAt.AddSeconds(expiresInSeconds);
            return token;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<(string Token, int ExpiresIn)> ExchangeAsync(CancellationToken ct)
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _options.PackageSid!),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret!),
            new KeyValuePair<string, string>("scope", Scope),
        });

        HttpResponseMessage response = await httpClient.PostAsync(TokenEndpoint, form, ct);
        response.EnsureSuccessStatusCode();

        TokenResponse? body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        if (body is null || string.IsNullOrEmpty(body.AccessToken))
        {
            throw new InvalidOperationException("Réponse OAuth2 WNS invalide (access_token manquant).");
        }

        return (body.AccessToken, body.ExpiresIn);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.PackageSid) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("Wns:PackageSid et Wns:ClientSecret sont requis.");
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
