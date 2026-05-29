using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Push.Apns;

/// <summary>
/// Signe et cache le provider authentication token APNs (F-020).
/// JWT ES256 : header <c>{alg:ES256, kid:KEY_ID, typ:JWT}</c>, payload <c>{iss:TEAM_ID, iat:now}</c>,
/// signature ECDSA-SHA256 avec la clé privée P-256 du .p8. Le token est mis en cache 30 min
/// (Apple plafonne à 1 h, recommande 20-60 min). Refresh thread-safe.
/// </summary>
internal sealed class ApnsAccessTokenProvider(IOptions<ApnsOptions> options)
    : IApnsAccessTokenProvider, IDisposable
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);

    private readonly ApnsOptions _options = options.Value;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _cachedExpiresAt;

    public void Dispose() => _refreshLock.Dispose();

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedExpiresAt)
        {
            return _cachedToken;
        }

        await _refreshLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedExpiresAt)
            {
                return _cachedToken;
            }

            EnsureConfigured();

            DateTimeOffset issuedAt = DateTimeOffset.UtcNow;
            _cachedToken = BuildSignedJwt(_options.TeamId!, _options.KeyId!, _options.PrivateKeyPem!, issuedAt);
            _cachedExpiresAt = issuedAt + TokenLifetime;
            return _cachedToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    internal static string BuildSignedJwt(string teamId, string keyId, string privateKeyPem, DateTimeOffset issuedAt)
    {
        var header = new { alg = "ES256", kid = keyId, typ = "JWT" };
        var payload = new { iss = teamId, iat = issuedAt.ToUnixTimeSeconds() };

        string headerB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        string payloadB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        string toSign = $"{headerB64}.{payloadB64}";

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKeyPem);
        // SignData renvoie déjà la signature au format IEEE P1363 (r||s) attendu par ES256.
        byte[] signature = ecdsa.SignData(Encoding.UTF8.GetBytes(toSign), HashAlgorithmName.SHA256);

        return $"{toSign}.{Base64UrlEncode(signature)}";
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.TeamId)
            || string.IsNullOrWhiteSpace(_options.KeyId)
            || string.IsNullOrWhiteSpace(_options.PrivateKeyPem))
        {
            throw new InvalidOperationException(
                "Apns:TeamId, Apns:KeyId et Apns:PrivateKeyPem sont requis.");
        }
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
