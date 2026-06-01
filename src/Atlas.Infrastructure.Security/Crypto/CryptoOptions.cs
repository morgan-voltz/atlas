namespace Atlas.Infrastructure.Security.Crypto;

public sealed class CryptoOptions
{
    public const string SectionName = "Crypto";

    /// <summary>Clé AES-256 (32 octets) encodée en Base64. Si absente, une clé DEV déterministe est utilisée.</summary>
    public string? KeyBase64 { get; set; }

    /// <summary>Chemin d'un fichier contenant la clé Base64 (secret monté à permissions restreintes, ADR-019).
    /// Chargé dans <see cref="KeyBase64"/> au démarrage s'il est vide.</summary>
    public string? KeyBase64File { get; set; }
}
