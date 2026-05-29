using Atlas.Domain.Users;

namespace Atlas.Application.Users.Accessibility;

/// <summary>
/// DTO transport pour les préférences d'accessibilité (cf. <c>docs/06-accessibilite.md</c> §6.3).
/// Exposé par les endpoints GET / PUT <c>/user/preferences/accessibility</c>.
/// </summary>
public sealed record AccessibilityPreferencesDto(
    bool HighContrast,
    bool ReduceMotion,
    AccessibilityFontPreference FontPreference);
