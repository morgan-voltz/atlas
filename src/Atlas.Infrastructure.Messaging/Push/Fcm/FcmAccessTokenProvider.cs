using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Push.Fcm;

/// <summary>
/// Échange un JWT signé RS256 (claim <c>iss</c> = service account email,
/// <c>scope</c> = <c>firebase.messaging</c>) contre un access token OAuth2 réutilisable
/// pendant ~1 h. Le token est mis en cache jusqu'à 5 min avant son expiration.
/// </summary>
internal sealed class FcmAccessTokenProvider(
    HttpClient httpClient,
    IOptions<FcmOptions> options) : IFcmAccessTokenProvider, IDisposable
{
    private const string Scope = "https://www.googleapis.com/auth/firebase.messaging";
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly Lazy<FcmServiceAccount> _serviceAccount = new(() => ParseServiceAccount(options.Value));

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

            (string token, DateTimeOffset expiresAt) = await ExchangeAsync(_serviceAccount.Value, ct);
            _cachedToken = token;
            _cachedExpiresAt = expiresAt;
            return token;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<(string Token, DateTimeOffset ExpiresAt)> ExchangeAsync(
        FcmServiceAccount account,
        CancellationToken ct)
    {
        DateTimeOffset issuedAt = DateTimeOffset.UtcNow;
        DateTimeOffset expiresAt = issuedAt.AddHours(1);

        string assertion = BuildSignedJwt(account, issuedAt, expiresAt);

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("assertion", assertion),
        });

        HttpResponseMessage response = await httpClient.PostAsync(account.TokenUri, form, ct);
        response.EnsureSuccessStatusCode();

        TokenResponse? body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        if (body is null || string.IsNullOrEmpty(body.AccessToken))
        {
            throw new InvalidOperationException("Réponse OAuth2 invalide (access_token manquant).");
        }

        return (body.AccessToken, issuedAt.AddSeconds(body.ExpiresIn));
    }

    internal static string BuildSignedJwt(FcmServiceAccount account, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        var header = new { alg = "RS256", typ = "JWT" };
        var payload = new
        {
            iss = account.ClientEmail,
            scope = Scope,
            aud = account.TokenUri,
            exp = expiresAt.ToUnixTimeSeconds(),
            iat = issuedAt.ToUnixTimeSeconds(),
        };

        string headerB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        string payloadB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        string toSign = $"{headerB64}.{payloadB64}";

        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(account.PrivateKey);
        byte[] signature = rsa.SignData(
            Encoding.UTF8.GetBytes(toSign),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{toSign}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static FcmServiceAccount ParseServiceAccount(FcmOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceAccountJson))
        {
            throw new InvalidOperationException("Fcm:ServiceAccountJson n'est pas configuré.");
        }

        FcmServiceAccount? account = JsonSerializer.Deserialize<FcmServiceAccount>(options.ServiceAccountJson);
        if (account is null
            || string.IsNullOrWhiteSpace(account.ClientEmail)
            || string.IsNullOrWhiteSpace(account.PrivateKey))
        {
            throw new InvalidOperationException(
                "Service account FCM invalide : champs client_email et private_key requis.");
        }

        return account;
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
