namespace Atlas.Infrastructure.Security.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "atlas";

    public string Audience { get; set; } = "atlas";

    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Clé privée RSA au format PEM (PKCS#8). Si absente, une clé éphémère est générée (DEV uniquement).</summary>
    public string? PrivateKeyPem { get; set; }
}
