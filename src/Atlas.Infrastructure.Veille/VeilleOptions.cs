namespace Atlas.Infrastructure.Veille;

public sealed class VeilleOptions
{
    public const string SectionName = "Veille";

    /// <summary>Timeout HTTP pour récupérer un flux.</summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>En-tête User-Agent envoyé aux serveurs de flux.</summary>
    public string UserAgent { get; set; } = "AtlasVeille/1.0 (+https://github.com/Morgan-Voltz/atlas)";
}
