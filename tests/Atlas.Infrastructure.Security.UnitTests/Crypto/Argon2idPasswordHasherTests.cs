using Atlas.Domain.Users;
using Atlas.Infrastructure.Security.Crypto;
using FluentAssertions;

namespace Atlas.Infrastructure.Security.UnitTests.Crypto;

public sealed class Argon2idPasswordHasherTests
{
    private readonly Argon2idPasswordHasher _hasher = new();

    [Fact]
    public void Hash_then_Verify_with_same_password_returns_true()
    {
        PasswordHash hash = _hasher.Hash("correct horse battery staple");

        _hasher.Verify("correct horse battery staple", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_with_wrong_password_returns_false()
    {
        PasswordHash hash = _hasher.Hash("correct horse battery staple");

        _hasher.Verify("WRONG", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_produces_different_outputs_for_same_input_thanks_to_unique_salt()
    {
        PasswordHash a = _hasher.Hash("same");
        PasswordHash b = _hasher.Hash("same");

        a.Value.Should().NotBe(b.Value);
    }

    [Fact]
    public void Verify_with_malformed_hash_returns_false_without_throwing()
    {
        // Format inattendu (pas de panique) : Verify renvoie simplement false.
        PasswordHash malformed = PasswordHash.FromHash("not-a-valid-hash-format");

        _hasher.Verify("anything", malformed).Should().BeFalse();
    }

    [Fact]
    public void Verify_with_invalid_base64_in_hash_returns_false()
    {
        PasswordHash malformed = PasswordHash.FromHash("65536.3.4.!!!.!!!");

        _hasher.Verify("anything", malformed).Should().BeFalse();
    }

    [Fact]
    public void Hash_value_contains_encoded_parameters()
    {
        // Format documenté : {memoryKb}.{iterations}.{parallelism}.{saltBase64}.{hashBase64}
        PasswordHash hash = _hasher.Hash("password");

        hash.Value.Split('.').Length.Should().Be(5);
        hash.Value.Should().StartWith("65536.3.4.");
    }
}
