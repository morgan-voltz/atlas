using Atlas.Infrastructure.Security.Crypto;
using FluentAssertions;

namespace Atlas.Infrastructure.Security.UnitTests.Crypto;

public sealed class SecureTokenGeneratorTests
{
    private readonly SecureTokenGenerator _generator = new();

    [Fact]
    public void GenerateUrlSafeToken_returns_url_safe_base64_string()
    {
        string token = _generator.GenerateUrlSafeToken();

        token.Should().NotBeNullOrEmpty();
        // Base64Url : pas de '+', '/', '=' (caractères non URL-safe du Base64 standard).
        token.Should().NotContainAny("+", "/", "=");
    }

    [Fact]
    public void GenerateUrlSafeToken_produces_different_values_each_call()
    {
        _generator.GenerateUrlSafeToken().Should().NotBe(_generator.GenerateUrlSafeToken());
    }

    [Fact]
    public void Hash_is_deterministic_for_same_input()
    {
        string hash1 = _generator.Hash("token-value");
        string hash2 = _generator.Hash("token-value");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_returns_uppercase_hex_of_sha256()
    {
        string hash = _generator.Hash("token-value");

        // SHA-256 → 32 octets → 64 caractères hex.
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9A-F]+$");
    }

    [Fact]
    public void Hash_different_inputs_produce_different_hashes()
    {
        _generator.Hash("a").Should().NotBe(_generator.Hash("b"));
    }

    [Fact]
    public void Hash_with_null_throws()
    {
        Action act = () => _generator.Hash(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
