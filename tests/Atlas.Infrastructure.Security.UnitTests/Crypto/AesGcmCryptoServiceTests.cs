using System.Security.Cryptography;
using Atlas.Infrastructure.Security.Crypto;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Security.UnitTests.Crypto;

public sealed class AesGcmCryptoServiceTests
{
    private static AesGcmCryptoService NewService(string? keyBase64) =>
        new(Options.Create(new CryptoOptions { KeyBase64 = keyBase64 }));

    private static string GenerateKey() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    [Fact]
    public void Encrypt_then_Decrypt_returns_original_plaintext()
    {
        AesGcmCryptoService service = NewService(GenerateKey());
        string original = "Identifiants INPI confidentiels";

        string cipher = service.Encrypt(original);
        string round = service.Decrypt(cipher);

        round.Should().Be(original);
    }

    [Fact]
    public void Encrypt_produces_different_outputs_for_same_plaintext_unique_nonce()
    {
        AesGcmCryptoService service = NewService(GenerateKey());

        string a = service.Encrypt("plaintext");
        string b = service.Encrypt("plaintext");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Decrypt_with_tampered_ciphertext_throws_cryptographic_exception()
    {
        AesGcmCryptoService service = NewService(GenerateKey());
        string cipher = service.Encrypt("plaintext");
        byte[] bytes = Convert.FromBase64String(cipher);
        bytes[^1] ^= 0x01;
        string tampered = Convert.ToBase64String(bytes);

        Action act = () => service.Decrypt(tampered);

        // Le tag GCM détecte toute altération du ciphertext OU du tag.
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_with_tampered_tag_throws_cryptographic_exception()
    {
        AesGcmCryptoService service = NewService(GenerateKey());
        string cipher = service.Encrypt("plaintext");
        byte[] bytes = Convert.FromBase64String(cipher);
        // Le tag est aux octets [12..28[ (nonce(12) || tag(16) || cipher).
        bytes[15] ^= 0x01;
        string tampered = Convert.ToBase64String(bytes);

        Action act = () => service.Decrypt(tampered);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_with_wrong_key_throws()
    {
        AesGcmCryptoService writer = NewService(GenerateKey());
        AesGcmCryptoService reader = NewService(GenerateKey()); // autre clé
        string cipher = writer.Encrypt("plaintext");

        Action act = () => reader.Decrypt(cipher);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Constructor_with_invalid_key_length_throws_invalid_operation()
    {
        string invalidKey = Convert.ToBase64String(new byte[16]); // 128 bits au lieu de 256

        Action act = () => NewService(invalidKey);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Default_dev_fallback_allows_roundtrip_for_local_development()
    {
        // Pas de clé configurée : le fallback DEV (clé statique) permet encrypt+decrypt en local.
        // En prod, AddSecurityInfrastructure (Lot 2a) fait échouer le démarrage si la clé est absente.
        AesGcmCryptoService service = NewService(keyBase64: null);

        service.Decrypt(service.Encrypt("dev")).Should().Be("dev");
    }
}
