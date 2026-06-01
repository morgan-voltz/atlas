namespace Atlas.Infrastructure.Security.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "atlas";

    public string Audience { get; set; } = "atlas";

    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Clé privée RSA au format PEM (PKCS#8). Si absente, une clé éphémère est générée (DEV uniquement).</summary>
    public string? PrivateKeyPem { get; set; }

    /// <summary>
    /// Chemin d'un fichier contenant la clé PEM. Utile quand le PEM multiligne ne passe pas en variable
    /// d'environnement (préprod ADR-019 : secret monté à permissions restreintes). Chargé dans
    /// <see cref="PrivateKeyPem"/> au démarrage si celui-ci est vide.
    /// </summary>
    public string? PrivateKeyPemFile { get; set; }
}
