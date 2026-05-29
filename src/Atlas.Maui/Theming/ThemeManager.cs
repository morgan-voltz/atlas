using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Atlas.Maui.Theming;

/// <summary>
/// Gere le theme actif (identite coloree) et le mode (clair/sombre/systeme).
/// Les deux axes sont independants. L'app reference uniquement des tokens
/// semantiques via {DynamicResource Primary}, etc. : un swap met tout a jour.
/// <para>
/// Lot 5c : porte aussi les preferences d'accessibilite (HighContrast force le
/// theme Contraste, ReduceMotion / FontPreference exposees comme flags globaux).
/// </para>
/// </summary>
public sealed class ThemeManager
{
    const string KeyTheme = "atlas.theme";
    const string KeyMode  = "atlas.mode"; // "Light" | "Dark" | "System"
    const string KeyHighContrast = "atlas.a11y.high_contrast";
    const string KeyReduceMotion = "atlas.a11y.reduce_motion";
    const string KeyFontPreference = "atlas.a11y.font_preference";

    ResourceDictionary? _current;

    static readonly Dictionary<(AppThemeId, bool), Func<ResourceDictionary>> Factory = new()
    {
        [(AppThemeId.Atlas, false)] = () => new Resources.Themes.AtlasLight(),
        [(AppThemeId.Atlas, true)]  = () => new Resources.Themes.AtlasDark(),
        [(AppThemeId.Ocean, false)] = () => new Resources.Themes.OceanLight(),
        [(AppThemeId.Ocean, true)]  = () => new Resources.Themes.OceanDark(),
        [(AppThemeId.Foret, false)] = () => new Resources.Themes.ForetLight(),
        [(AppThemeId.Foret, true)]  = () => new Resources.Themes.ForetDark(),
        [(AppThemeId.Ambre, false)] = () => new Resources.Themes.AmbreLight(),
        [(AppThemeId.Ambre, true)]  = () => new Resources.Themes.AmbreDark(),
        [(AppThemeId.Amethyste, false)] = () => new Resources.Themes.AmethysteLight(),
        [(AppThemeId.Amethyste, true)]  = () => new Resources.Themes.AmethysteDark(),
        [(AppThemeId.Contraste, false)] = () => new Resources.Themes.ContrasteLight(),
        [(AppThemeId.Contraste, true)]  = () => new Resources.Themes.ContrasteDark(),
        [(AppThemeId.Sepia, false)] = () => new Resources.Themes.SepiaLight(),
        [(AppThemeId.Sepia, true)]  = () => new Resources.Themes.SepiaDark(),
    };

    public AppThemeId Theme { get; private set; } = AppThemeId.Atlas;
    public AppTheme Mode { get; private set; } = AppTheme.Unspecified; // Unspecified = suit le systeme

    /// <summary>
    /// Lot 5c : force le theme « Contraste » si activee. La preference Theme reste memorisee
    /// (restauree au desactivation), aucun choix utilisateur n'est perdu.
    /// </summary>
    public bool HighContrast { get; private set; }

    /// <summary>
    /// Lot 5c : flag global a respecter par les vues / animations (transitions, parallax).
    /// Persiste cross-session ; les futurs composants animes doivent l'interroger.
    /// </summary>
    public bool ReduceMotion { get; private set; }

    /// <summary>
    /// Lot 5c : preference de police facilitante. Aucun asset embarque pour l'instant
    /// (OpenDyslexic / Atkinson Hyperlegible non packagees) — la preference est persistee
    /// et synchronisee multi-device, application visuelle dans une PR ulterieure.
    /// </summary>
    public string FontPreference { get; private set; } = "Default";

    /// <summary>A appeler au demarrage (App.xaml.cs) pour restaurer le choix.</summary>
    public void Initialize()
    {
        Theme = Enum.TryParse(Preferences.Get(KeyTheme, nameof(AppThemeId.Atlas)),
                              out AppThemeId t) ? t : AppThemeId.Atlas;
        Mode = Preferences.Get(KeyMode, "System") switch
        {
            "Light" => AppTheme.Light,
            "Dark"  => AppTheme.Dark,
            _        => AppTheme.Unspecified,
        };
        HighContrast = Preferences.Get(KeyHighContrast, false);
        ReduceMotion = Preferences.Get(KeyReduceMotion, false);
        FontPreference = Preferences.Get(KeyFontPreference, "Default");
        Apply();
    }

    public void SetTheme(AppThemeId theme)
    {
        Theme = theme;
        Preferences.Set(KeyTheme, theme.ToString());
        Apply();
    }

    public void SetMode(AppTheme mode)
    {
        Mode = mode;
        Preferences.Set(KeyMode, mode == AppTheme.Light ? "Light"
                               : mode == AppTheme.Dark ? "Dark" : "System");
        Apply();
    }

    /// <summary>
    /// Lot 5c : applique les preferences d'accessibilite recues du backend ou choisies localement.
    /// HighContrast remplace temporairement le theme actif par Contraste sans ecraser la
    /// preference Theme de l'utilisateur (revert transparent).
    /// </summary>
    public void SetAccessibility(bool highContrast, bool reduceMotion, string fontPreference)
    {
        ArgumentNullException.ThrowIfNull(fontPreference);

        HighContrast = highContrast;
        ReduceMotion = reduceMotion;
        FontPreference = fontPreference;
        Preferences.Set(KeyHighContrast, highContrast);
        Preferences.Set(KeyReduceMotion, reduceMotion);
        Preferences.Set(KeyFontPreference, fontPreference);
        Apply();
    }

    /// <summary>Resout clair/sombre effectif et echange le dictionnaire actif.</summary>
    public void Apply()
    {
        var app = Application.Current;
        if (app is null) return;

        // Force le mode demande (sinon suit le systeme)
        app.UserAppTheme = Mode;

        bool isDark = Mode switch
        {
            AppTheme.Light => false,
            AppTheme.Dark  => true,
            _ => app.RequestedTheme == AppTheme.Dark,
        };

        // Lot 5c : HighContrast force le theme Contraste sans ecraser la preference Theme.
        AppThemeId effectiveTheme = HighContrast ? AppThemeId.Contraste : Theme;
        var next = Factory[(effectiveTheme, isDark)]();

        var dicts = app.Resources.MergedDictionaries;
        if (_current is not null) dicts.Remove(_current);
        dicts.Add(next);
        _current = next;

        // Lot 5d : applique la police facilitante. Mise a jour de la ressource racine
        // AppFontFamily (referencee par les Style globaux via DynamicResource) — un
        // changement propage l'effet sur toutes les vues sans recompilation des styles.
        app.Resources["AppFontFamily"] = ResolveFontFamily(FontPreference);
    }

    /// <summary>
    /// Mappe une preference utilisateur ("DyslexiaFriendly", "HighReadability", ...) sur
    /// le nom de FontFamily enregistre dans MauiProgram. Inconnue → defaut OpenSans.
    /// </summary>
    public static string ResolveFontFamily(string fontPreference) => fontPreference switch
    {
        "DyslexiaFriendly" => "OpenDyslexicRegular",
        "HighReadability"  => "AtkinsonHyperlegibleRegular",
        _ => "OpenSansRegular",
    };
}
