using System.Security.Cryptography;
using System.Text;
using Atlas.Domain.Security;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Security.Crypto;

/// <summary>
/// Chiffrement au repos AES-256-GCM. Format de sortie : Base64(nonce[12] || tag[16] || ciphertext).
/// La clé provient de la configuration (idéalement injectée depuis un KMS) ; à défaut, une clé DEV
/// déterministe est dérivée — acceptable en développement uniquement.
/// </summary>
internal sealed class AesGcmCryptoService : ICryptoService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;

    private readonly byte[] _key;

    public AesGcmCryptoService(IOptions<CryptoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        string? keyBase64 = options.Value.KeyBase64;

        if (!string.IsNullOrWhiteSpace(keyBase64))
        {
            _key = Convert.FromBase64String(keyBase64);
            if (_key.Length != KeySize)
            {
                throw new InvalidOperationException(
                    $"La clé Crypto:KeyBase64 doit faire {KeySize} octets (AES-256) une fois décodée.");
            }
        }
        else
        {
            // DEV uniquement : clé déterministe non sécurisée. En production, configurer Crypto:KeyBase64 (KMS).
            _key = SHA256.HashData(Encoding.UTF8.GetBytes("atlas-dev-insecure-crypto-key"));
        }
    }

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        byte[] plain = Encoding.UTF8.GetBytes(plaintext);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] cipher = new byte[plain.Length];
        byte[] tag = new byte[TagSize];

        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Encrypt(nonce, plain, cipher, tag);
        }

        byte[] output = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, output, NonceSize, TagSize);
        Buffer.BlockCopy(cipher, 0, output, NonceSize + TagSize, cipher.Length);

        return Convert.ToBase64String(output);
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);

        byte[] input = Convert.FromBase64String(ciphertext);
        byte[] nonce = input[..NonceSize];
        byte[] tag = input[NonceSize..(NonceSize + TagSize)];
        byte[] cipher = input[(NonceSize + TagSize)..];
        byte[] plain = new byte[cipher.Length];

        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Decrypt(nonce, cipher, tag, plain);
        }

        return Encoding.UTF8.GetString(plain);
    }
}
