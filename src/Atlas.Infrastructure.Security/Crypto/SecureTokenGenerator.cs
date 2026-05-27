using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Atlas.Domain.Security;

namespace Atlas.Infrastructure.Security.Crypto;

/// <summary>
/// Génère des jetons opaques cryptographiquement sûrs (URL-safe) et calcule leur hash de stockage (SHA-256).
/// </summary>
internal sealed class SecureTokenGenerator : ITokenGenerator
{
    public string GenerateUrlSafeToken(int byteLength = 32)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base64Url.EncodeToString(bytes);
    }

    public string Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
