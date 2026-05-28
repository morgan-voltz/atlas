namespace Atlas.Api.Security;

/// <summary>
/// Origines autorisées pour CORS. Lue depuis la section "Cors" de la configuration.
/// Liste vide tolérée en Development uniquement (pas de CORS du tout) ;
/// hors Development, au moins une origine est requise (le démarrage échoue sinon).
/// </summary>
internal sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public const string DefaultPolicyName = "AtlasDefault";

    /// <summary>Origines explicitement autorisées. Jamais <c>*</c>.</summary>
    public IList<string> AllowedOrigins { get; set; } = [];
}
