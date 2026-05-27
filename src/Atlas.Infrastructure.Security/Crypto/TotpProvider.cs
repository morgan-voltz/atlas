using Atlas.Domain.Security;
using OtpNet;

namespace Atlas.Infrastructure.Security.Crypto;

/// <summary>
/// TOTP RFC 6238 via Otp.NET. Secret en Base32, fenêtre de tolérance ±1 pas (30 s) pour absorber la dérive d'horloge.
/// </summary>
internal sealed class TotpProvider : ITotpProvider
{
    private const int SecretByteLength = 20;
    private static readonly VerificationWindow Window = new(previous: 1, future: 1);

    public string GenerateSecret()
    {
        byte[] key = KeyGeneration.GenerateRandomKey(SecretByteLength);
        return Base32Encoding.ToString(key);
    }

    public string BuildProvisioningUri(string secret, string accountName, string issuer)
    {
        string label = Uri.EscapeDataString($"{issuer}:{accountName}");
        string escapedIssuer = Uri.EscapeDataString(issuer);
        return $"otpauth://totp/{label}?secret={secret}&issuer={escapedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out long _, Window);
    }
}
