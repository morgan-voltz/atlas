namespace Atlas.Domain.Users;

/// <summary>
/// Préférences d'accessibilité portées par l'utilisateur et synchronisées multi-device
/// (cf. <c>docs/06-accessibilite.md</c> §6.3 — endpoint API dédié pour les retrouver sur tous
/// les terminaux). Owned type EF Core sur <see cref="User"/>.
/// </summary>
/// <param name="HighContrast">
/// Force le thème à haut contraste (mappé côté MAUI sur le thème « Contraste » du kit thèmes
/// WCAG 2.2 AA).
/// </param>
/// <param name="ReduceMotion">
/// Désactive les animations non essentielles (transitions de pages, parallax, micro-animations).
/// </param>
/// <param name="FontPreference">
/// Police facilitante (cf. <see cref="AccessibilityFontPreference"/>).
/// </param>
public sealed record UserAccessibilityPreferences(
    bool HighContrast,
    bool ReduceMotion,
    AccessibilityFontPreference FontPreference)
{
    /// <summary>Préférences neutres : aucun ajustement par rapport au défaut applicatif.</summary>
    public static UserAccessibilityPreferences Default { get; } =
        new(HighContrast: false, ReduceMotion: false, FontPreference: AccessibilityFontPreference.Default);
}
