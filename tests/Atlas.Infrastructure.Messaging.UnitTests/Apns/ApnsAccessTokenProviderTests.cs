using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Infrastructure.Messaging.Push.Apns;
using FluentAssertions;

namespace Atlas.Infrastructure.Messaging.UnitTests.Apns;

public sealed class ApnsAccessTokenProviderTests
{
    private static readonly DateTimeOffset IssuedAt = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void BuildSignedJwt_produces_three_segments_with_ES256_header()
    {
        string privateKeyPem = NewP256PrivateKeyPem(out _);

        string jwt = ApnsAccessTokenProvider.BuildSignedJwt("TEAMID1234", "KEYID5678", privateKeyPem, IssuedAt);

        string[] parts = jwt.Split('.');
        parts.Should().HaveCount(3);

        string headerJson = Encoding.UTF8.GetString(FromBase64Url(parts[0]));
        using JsonDocument doc = JsonDocument.Parse(headerJson);
        doc.RootElement.GetProperty("alg").GetString().Should().Be("ES256");
        doc.RootElement.GetProperty("kid").GetString().Should().Be("KEYID5678");
        doc.RootElement.GetProperty("typ").GetString().Should().Be("JWT");
    }

    [Fact]
    public void BuildSignedJwt_payload_contains_iss_and_iat()
    {
        string privateKeyPem = NewP256PrivateKeyPem(out _);

        string jwt = ApnsAccessTokenProvider.BuildSignedJwt("TEAMID1234", "KEYID5678", privateKeyPem, IssuedAt);

        string payloadJson = Encoding.UTF8.GetString(FromBase64Url(jwt.Split('.')[1]));
        using JsonDocument doc = JsonDocument.Parse(payloadJson);
        doc.RootElement.GetProperty("iss").GetString().Should().Be("TEAMID1234");
        doc.RootElement.GetProperty("iat").GetInt64().Should().Be(IssuedAt.ToUnixTimeSeconds());
    }

    [Fact]
    public void BuildSignedJwt_signature_is_verifiable_with_public_key()
    {
        string privateKeyPem = NewP256PrivateKeyPem(out ECDsa ecdsaForVerify);

        string jwt = ApnsAccessTokenProvider.BuildSignedJwt("TEAMID1234", "KEYID5678", privateKeyPem, IssuedAt);

        string[] parts = jwt.Split('.');
        byte[] toVerify = Encoding.UTF8.GetBytes(parts[0] + "." + parts[1]);
        byte[] signature = FromBase64Url(parts[2]);

        // Le format ES256 attendu est IEEE P1363 (r||s), ce que SignData renvoie déjà.
        ecdsaForVerify.VerifyData(toVerify, signature, HashAlgorithmName.SHA256)
            .Should().BeTrue();
    }

    private static string NewP256PrivateKeyPem(out ECDsa ecdsa)
    {
        ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return ecdsa.ExportPkcs8PrivateKeyPem();
    }

    private static byte[] FromBase64Url(string input)
    {
        string padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}
