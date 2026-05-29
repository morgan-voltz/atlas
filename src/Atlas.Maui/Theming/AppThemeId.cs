namespace Atlas.Maui.Theming;

/// <summary>Les 7 themes selectionnables par l'utilisateur.</summary>
public enum AppThemeId
{
    Atlas,
    Ocean,
    Foret,
    Ambre,
    Amethyste,
    Contraste,
    Sepia
}

public static class AppThemeIdExtensions
{
    /// <summary>Libelle affichable dans le selecteur de theme.</summary>
    public static string ToLabel(this AppThemeId id) => id switch
    {
        AppThemeId.Atlas => "Atlas",
        AppThemeId.Ocean => "Ocean",
        AppThemeId.Foret => "Foret",
        AppThemeId.Ambre => "Ambre",
        AppThemeId.Amethyste => "Amethyste",
        AppThemeId.Contraste => "Contraste eleve",
        AppThemeId.Sepia => "Sepia (faible lumiere bleue)",
        _ => id.ToString(),
    };
}
