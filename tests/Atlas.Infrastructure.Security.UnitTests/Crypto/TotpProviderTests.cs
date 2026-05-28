using Atlas.Infrastructure.Security.Crypto;
using FluentAssertions;
using OtpNet;

namespace Atlas.Infrastructure.Security.UnitTests.Crypto;

public sealed class TotpProviderTests
{
    private readonly TotpProvider _provider = new();

    [Fact]
    public void GenerateSecret_returns_a_Base32_encoded_160_bit_secret()
    {
        string secret = _provider.GenerateSecret();

        secret.Should().NotBeNullOrEmpty();
        byte[] decoded = Base32Encoding.ToBytes(secret);
        decoded.Length.Should().Be(20); // 160 bits, conforme RFC 6238
    }

    [Fact]
    public void GenerateSecret_returns_different_values_each_call()
    {
        _provider.GenerateSecret().Should().NotBe(_provider.GenerateSecret());
    }

    [Fact]
    public void BuildProvisioningUri_contains_secret_issuer_algorithm_digits_period()
    {
        string secret = _provider.GenerateSecret();

        string uri = _provider.BuildProvisioningUri(secret, "alice@example.com", "Atlas");

        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain($"secret={secret}");
        uri.Should().Contain("issuer=Atlas");
        uri.Should().Contain("algorithm=SHA1");
        uri.Should().Contain("digits=6");
        uri.Should().Contain("period=30");
    }

    [Fact]
    public void VerifyCode_accepts_a_code_computed_for_the_current_time()
    {
        string secret = _provider.GenerateSecret();
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        string code = totp.ComputeTotp();

        _provider.VerifyCode(secret, code).Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_rejects_a_random_code()
    {
        string secret = _provider.GenerateSecret();

        _provider.VerifyCode(secret, "000000").Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyCode_rejects_empty_or_whitespace_code(string? code)
    {
        string secret = _provider.GenerateSecret();

        _provider.VerifyCode(secret, code!).Should().BeFalse();
    }
}
