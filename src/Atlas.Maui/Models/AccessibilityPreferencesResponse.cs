namespace Atlas.Maui.Models;

/// <summary>
/// DTO transport des préférences d'accessibilité (cf. <c>docs/06-accessibilite.md</c> §6.3).
/// Format aligné sur l'endpoint API <c>/user/preferences/accessibility</c>.
/// </summary>
public sealed record AccessibilityPreferencesResponse(
    bool HighContrast,
    bool ReduceMotion,
    string FontPreference);
