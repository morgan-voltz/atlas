using System.Xml.Linq;
using FluentAssertions;

namespace Atlas.Architecture.Tests;

/// <summary>
/// Tests d'architecture qui parsent directement les fichiers <c>.csproj</c>.
/// Utile pour les projets qui ne peuvent pas être référencés depuis un projet de tests
/// classique <c>net10.0</c> :
/// <list type="bullet">
///   <item><description><c>Atlas.Maui</c> multi-target <c>net10.0-{android,ios,maccatalyst,windows}</c></description></item>
///   <item><description><c>Atlas.Infrastructure.Bodacc</c> non référencé pour limiter la taille de la build des tests</description></item>
/// </list>
/// Couvre Lot 6 — Lot 3 résiduel de l'audit profond.
/// </summary>
public class CsprojDependencyTests
{
    /// <summary>
    /// CLAUDE.md — Cas particulier `Atlas.Maui` : le client mobile ne doit JAMAIS référencer
    /// <c>Atlas.Infrastructure.*</c> ni <c>Atlas.Application</c>. Le code serait livré sur les
    /// terminaux des utilisateurs et pourrait être décompilé. Toute interaction passe par
    /// l'API HTTP via <c>AtlasApiClient</c>. Seules <c>Atlas.Domain</c> et <c>Atlas.Shared</c>
    /// sont autorisées.
    /// </summary>
    [Fact]
    public void Atlas_Maui_csproj_should_only_reference_Domain_and_Shared()
    {
        List<string> references = ReadProjectReferences("src/Atlas.Maui/Atlas.Maui.csproj");

        references.Should().NotContain(
            r => r.Contains("Atlas.Application", StringComparison.Ordinal),
            "Atlas.Maui ne doit pas embarquer la couche Application (handlers métier, ports) — risque de décompilation.");

        references.Should().NotContain(
            r => r.Contains("Atlas.Infrastructure", StringComparison.Ordinal),
            "Atlas.Maui ne doit pas embarquer un adapter d'infrastructure (HttpClient INPI, EF, KMS…) — risque de décompilation. Voir CLAUDE.md.");

        references.Should().NotContain(
            r => r.Contains("Atlas.Api", StringComparison.Ordinal),
            "Atlas.Maui ne doit pas référencer la composition root API.");

        // Et ne référence que Domain + Shared (en plus des packages NuGet).
        IEnumerable<string> atlasReferences = references.Where(r => r.Contains("Atlas.", StringComparison.Ordinal));
        atlasReferences.Should().OnlyContain(
            r => r.Contains("Atlas.Domain", StringComparison.Ordinal) || r.Contains("Atlas.Shared", StringComparison.Ordinal),
            "Atlas.Maui ne doit référencer que Atlas.Domain et Atlas.Shared.");
    }

    /// <summary>
    /// ADR-029 / ADR-002 — Cas particulier `Atlas.App` (client Uno multi-cible, single project) :
    /// même règle que <c>Atlas.Maui</c>, et tout particulièrement pour la tête WebAssembly
    /// (code décompilable dans le navigateur). Le client ne doit JAMAIS référencer
    /// <c>Atlas.Infrastructure.*</c>, <c>Atlas.Application</c> ni <c>Atlas.Api</c> — uniquement
    /// <c>Atlas.Domain</c> et <c>Atlas.Shared</c>, toute interaction passant par l'API HTTP
    /// (<c>AtlasApiClient</c>). Testé au niveau csproj car le projet est multi-cible
    /// (net10.0-browserwasm/desktop…) et non référençable depuis ce projet net10.0.
    /// </summary>
    [Fact]
    public void Atlas_App_csproj_should_only_reference_Domain_and_Shared()
    {
        List<string> references = ReadProjectReferences("src/Atlas.App/Atlas.App.csproj");

        references.Should().NotContain(
            r => r.Contains("Atlas.Application", StringComparison.Ordinal),
            "Atlas.App ne doit pas embarquer la couche Application (handlers métier, ports) — risque de décompilation.");

        references.Should().NotContain(
            r => r.Contains("Atlas.Infrastructure", StringComparison.Ordinal),
            "Atlas.App ne doit pas embarquer un adapter d'infrastructure (HttpClient INPI, EF, KMS…) — risque de décompilation, surtout côté WASM. Voir CLAUDE.md / ADR-029.");

        references.Should().NotContain(
            r => r.Contains("Atlas.Api", StringComparison.Ordinal),
            "Atlas.App ne doit pas référencer la composition root API.");

        IEnumerable<string> atlasReferences = references.Where(r => r.Contains("Atlas.", StringComparison.Ordinal));
        atlasReferences.Should().OnlyContain(
            r => r.Contains("Atlas.Domain", StringComparison.Ordinal) || r.Contains("Atlas.Shared", StringComparison.Ordinal),
            "Atlas.App ne doit référencer que Atlas.Domain et Atlas.Shared.");
    }

    /// <summary>
    /// Aucune Atlas.Infrastructure.* ne doit en référencer une autre. La composition se fait
    /// dans <c>Atlas.Api</c> (composition root). Cross-référence = ports & adapters cassés —
    /// un adapter en deviendrait un autre.
    /// </summary>
    [Theory]
    [InlineData("src/Atlas.Infrastructure.Persistence/Atlas.Infrastructure.Persistence.csproj")]
    [InlineData("src/Atlas.Infrastructure.Inpi/Atlas.Infrastructure.Inpi.csproj")]
    [InlineData("src/Atlas.Infrastructure.Veille/Atlas.Infrastructure.Veille.csproj")]
    [InlineData("src/Atlas.Infrastructure.Messaging/Atlas.Infrastructure.Messaging.csproj")]
    [InlineData("src/Atlas.Infrastructure.Security/Atlas.Infrastructure.Security.csproj")]
    [InlineData("src/Atlas.Infrastructure.Cache/Atlas.Infrastructure.Cache.csproj")]
    [InlineData("src/Atlas.Infrastructure.Storage/Atlas.Infrastructure.Storage.csproj")]
    [InlineData("src/Atlas.Infrastructure.Bodacc/Atlas.Infrastructure.Bodacc.csproj")]
    public void Infrastructure_csproj_should_not_reference_other_infrastructure_or_api(string relativeCsprojPath)
    {
        List<string> references = ReadProjectReferences(relativeCsprojPath);

        // Pas d'auto-référence Api.
        references.Should().NotContain(
            r => r.Contains("Atlas.Api", StringComparison.Ordinal),
            $"{relativeCsprojPath} ne doit pas référencer Atlas.Api (composition root).");

        // Pas de référence à un autre Atlas.Infrastructure.*.
        string ownName = Path.GetFileNameWithoutExtension(relativeCsprojPath);
        IEnumerable<string> infraCrossReferences = references
            .Where(r => r.Contains("Atlas.Infrastructure.", StringComparison.Ordinal))
            .Where(r => !r.Contains(ownName, StringComparison.Ordinal));

        infraCrossReferences.Should().BeEmpty(
            $"{ownName} ne doit pas référencer un autre Atlas.Infrastructure.* — la composition se fait dans Atlas.Api.");

        // Pas non plus de référence à Atlas.Maui.
        references.Should().NotContain(
            r => r.Contains("Atlas.Maui", StringComparison.Ordinal),
            $"{ownName} ne doit pas référencer Atlas.Maui.");
    }

    private static List<string> ReadProjectReferences(string relativeCsprojPath)
    {
        string repoRoot = FindRepoRoot();
        string fullPath = Path.Combine(repoRoot, relativeCsprojPath);
        File.Exists(fullPath).Should().BeTrue($"le csproj {relativeCsprojPath} doit exister (cherché à {fullPath}).");

        XDocument doc = XDocument.Load(fullPath);
        return doc.Descendants("ProjectReference")
            .Select(node => (string?)node.Attribute("Include"))
            .Where(include => !string.IsNullOrEmpty(include))
            .Select(include => include!)
            .ToList();
    }

    /// <summary>
    /// Remonte les dossiers depuis <see cref="AppContext.BaseDirectory"/> jusqu'au dossier contenant
    /// <c>Atlas.slnx</c>. Évite un chemin relatif fragile au profil de build.
    /// </summary>
    private static string FindRepoRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Atlas.slnx")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            $"Atlas.slnx introuvable à partir de {AppContext.BaseDirectory}");
    }
}
