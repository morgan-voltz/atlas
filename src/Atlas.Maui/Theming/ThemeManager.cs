using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Atlas.Maui.Theming;

/// <summary>
/// Gere le theme actif (identite coloree) et le mode (clair/sombre/systeme).
/// Les deux axes sont independants. L'app reference uniquement des tokens
/// semantiques via {DynamicResource Primary}, etc. : un swap met tout a jour.
/// </summary>
public sealed class ThemeManager
{
    const string KeyTheme = "atlas.theme";
    const string KeyMode  = "atlas.mode"; // "Light" | "Dark" | "System"

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

        var next = Factory[(Theme, isDark)]();

        var dicts = app.Resources.MergedDictionaries;
        if (_current is not null) dicts.Remove(_current);
        dicts.Add(next);
        _current = next;
    }
}
