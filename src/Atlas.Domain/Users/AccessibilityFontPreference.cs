using System.Text.Json.Serialization;

namespace Atlas.Domain.Users;

/// <summary>
/// Préférence de police facilitante côté client (cf. <c>docs/06-accessibilite.md</c> §10.3).
/// Application au runtime côté client MAUI (chargement de la <see cref="DyslexiaFriendly"/> ou
/// <see cref="HighReadability"/> via embedded font si disponible, sinon meilleur fallback système).
/// La préférence est persistée et synchronisée multi-device même si l'asset font n'est pas
/// encore embarqué : la valeur reste portée par l'utilisateur.
/// <para>
/// Sérialisée JSON en chaîne (« DyslexiaFriendly ») et non en entier — l'API expose un contrat
/// stable et lisible plutôt que des ordinaux d'enum fragiles aux insertions futures.
/// </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AccessibilityFontPreference>))]
public enum AccessibilityFontPreference
{
    /// <summary>Police par défaut (OpenSans dans Atlas MAUI).</summary>
    Default = 0,

    /// <summary>Police facilitante dyslexie (OpenDyslexic — SIL OFL 1.1).</summary>
    DyslexiaFriendly = 1,

    /// <summary>Police haute lisibilité (Atkinson Hyperlegible — SIL OFL 1.1, Braille Institute).</summary>
    HighReadability = 2,
}
