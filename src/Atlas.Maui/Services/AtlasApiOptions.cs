using Microsoft.Maui.Storage;

namespace Atlas.Maui.Services;

/// <summary>
/// Résolution de l'URL de base de l'API Atlas pour le client MAUI.
/// <para>
/// Stratégie en deux couches :
/// </para>
/// <list type="number">
///   <item>
///     <description>
///       Override utilisateur via <see cref="Preferences"/> (clé <see cref="PreferenceKey"/>).
///       Permet à un opérateur / testeur de pointer vers un backend distinct (staging, prod auto-hébergée…)
///       sans recompiler. Persisté par <see cref="Preferences"/> (par plateforme : SharedPreferences,
///       NSUserDefaults, Windows AppData, etc.).
///     </description>
///   </item>
///   <item>
///     <description>
///       Sinon, valeur par défaut <see cref="DefaultBaseUrlForPlatform"/> choisie à la compilation
///       selon la plateforme cible. Adresse l'asymétrie connue de l'émulateur Android
///       (<c>10.0.2.2</c> = host machine vs <c>localhost</c> ailleurs).
///     </description>
///   </item>
/// </list>
/// </summary>
internal sealed class AtlasApiOptions
{
    public const string PreferenceKey = "Atlas.BaseUrl";

    public AtlasApiOptions()
    {
        BaseUrl = ResolveBaseUrl();
    }

    public string BaseUrl { get; }

    private static string ResolveBaseUrl()
    {
        string userOverride = Preferences.Default.Get(PreferenceKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(userOverride))
        {
            return userOverride;
        }

        return DefaultBaseUrlForPlatform;
    }

    private static string DefaultBaseUrlForPlatform =>
#if ANDROID
        // Émulateur Android : 10.0.2.2 = localhost de la machine hôte.
        "https://10.0.2.2:7201/";
#elif IOS || MACCATALYST
        // Simulateur iOS / MacCatalyst : localhost direct.
        "https://localhost:7201/";
#elif WINDOWS
        "https://localhost:7201/";
#else
        "https://localhost:7201/";
#endif
}
