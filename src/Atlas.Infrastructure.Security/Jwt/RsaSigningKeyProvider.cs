using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Atlas.Infrastructure.Security.Jwt;

/// <summary>
/// Fournit une clé RSA pour la signature RS256. Charge une clé PEM configurée, ou génère une clé
/// éphémère 3072 bits en l'absence de configuration (acceptable en DEV uniquement — en production,
/// configurer une clé persistée, idéalement via KMS).
/// </summary>
internal sealed class RsaSigningKeyProvider : ISigningKeyProvider, IDisposable
{
    private readonly RSA _rsa;

    public RsaSigningKeyProvider(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        JwtOptions value = options.Value;

        _rsa = RSA.Create(3072);
        if (!string.IsNullOrWhiteSpace(value.PrivateKeyPem))
        {
            _rsa.ImportFromPem(value.PrivateKeyPem);
        }

        SigningKey = new RsaSecurityKey(_rsa) { KeyId = "atlas-rsa-1" };
        Issuer = value.Issuer;
        Audience = value.Audience;
    }

    public SecurityKey SigningKey { get; }

    public string Algorithm => SecurityAlgorithms.RsaSha256;

    public string Issuer { get; }

    public string Audience { get; }

    public void Dispose() => _rsa.Dispose();
}
