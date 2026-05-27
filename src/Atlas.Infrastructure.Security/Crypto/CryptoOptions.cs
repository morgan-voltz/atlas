namespace Atlas.Infrastructure.Security.Crypto;

public sealed class CryptoOptions
{
    public const string SectionName = "Crypto";

    /// <summary>Clé AES-256 (32 octets) encodée en Base64. Si absente, une clé DEV déterministe est utilisée.</summary>
    public string? KeyBase64 { get; set; }
}
