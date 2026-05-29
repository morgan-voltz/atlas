using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atlas.Infrastructure.Messaging.Push.Fcm;
using FluentAssertions;

namespace Atlas.Infrastructure.Messaging.UnitTests.Fcm;

public sealed class FcmAccessTokenProviderTests
{
    private static readonly DateTimeOffset IssuedAt = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ExpiresAt = IssuedAt.AddHours(1);

    [Fact]
    public void BuildSignedJwt_produces_three_segments()
    {
        FcmServiceAccount account = NewAccountWithFreshKey(out _);

        string jwt = FcmAccessTokenProvider.BuildSignedJwt(account, IssuedAt, ExpiresAt);

        string[] parts = jwt.Split('.');
        parts.Should().HaveCount(3);
    }

    [Fact]
    public void BuildSignedJwt_header_declares_RS256()
    {
        FcmServiceAccount account = NewAccountWithFreshKey(out _);

        string jwt = FcmAccessTokenProvider.BuildSignedJwt(account, IssuedAt, ExpiresAt);

        string headerJson = Encoding.UTF8.GetString(FromBase64Url(jwt.Split('.')[0]));
        using JsonDocument doc = JsonDocument.Parse(headerJson);
        doc.RootElement.GetProperty("alg").GetString().Should().Be("RS256");
        doc.RootElement.GetProperty("typ").GetString().Should().Be("JWT");
    }

    [Fact]
    public void BuildSignedJwt_payload_contains_claims()
    {
        FcmServiceAccount account = NewAccountWithFreshKey(out _);
        account.ClientEmail = "atlas@firebase.example";
        account.TokenUri = "https://oauth2.googleapis.com/token";

        string jwt = FcmAccessTokenProvider.BuildSignedJwt(account, IssuedAt, ExpiresAt);

        string payloadJson = Encoding.UTF8.GetString(FromBase64Url(jwt.Split('.')[1]));
        using JsonDocument doc = JsonDocument.Parse(payloadJson);
        doc.RootElement.GetProperty("iss").GetString().Should().Be("atlas@firebase.example");
        doc.RootElement.GetProperty("scope").GetString().Should().Be("https://www.googleapis.com/auth/firebase.messaging");
        doc.RootElement.GetProperty("aud").GetString().Should().Be("https://oauth2.googleapis.com/token");
        doc.RootElement.GetProperty("iat").GetInt64().Should().Be(IssuedAt.ToUnixTimeSeconds());
        doc.RootElement.GetProperty("exp").GetInt64().Should().Be(ExpiresAt.ToUnixTimeSeconds());
    }

    [Fact]
    public void BuildSignedJwt_signature_is_verifiable_with_public_key()
    {
        FcmServiceAccount account = NewAccountWithFreshKey(out RSA rsa);

        string jwt = FcmAccessTokenProvider.BuildSignedJwt(account, IssuedAt, ExpiresAt);

        string[] parts = jwt.Split('.');
        byte[] toVerify = Encoding.UTF8.GetBytes(parts[0] + "." + parts[1]);
        byte[] signature = FromBase64Url(parts[2]);
        rsa.VerifyData(toVerify, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            .Should().BeTrue();
    }

    /// <summary>Génère une paire RSA temporaire et une instance FcmServiceAccount valide.</summary>
    private static FcmServiceAccount NewAccountWithFreshKey(out RSA rsa)
    {
        rsa = RSA.Create(2048);
        string privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
        return new FcmServiceAccount
        {
            ClientEmail = "test@firebase.example",
            PrivateKey = privateKeyPem,
            TokenUri = "https://oauth2.googleapis.com/token",
        };
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
