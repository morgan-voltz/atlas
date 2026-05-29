namespace Atlas.Maui.Theming;

/// <summary>
/// Coordinateur d'animations qui respecte la préférence utilisateur <c>ReduceMotion</c>
/// (cf. <c>docs/06-accessibilite.md</c> §10.1 — pas d'information temporelle stricte, alternatives
/// aux gestes / animations obligatoires). Toute animation MAUI non essentielle (FadeTo,
/// TranslateTo, ScaleTo, transitions custom) doit passer par ce port plutôt que d'appeler
/// directement les extensions <c>VisualElement</c>.
/// <para>
/// Quand <see cref="MotionEnabled"/> vaut <c>false</c>, les méthodes appliquent l'état final
/// instantanément (aucune frame d'animation) — l'utilisateur ne perd ni l'information ni
/// le résultat visuel, seul le mouvement disparaît.
/// </para>
/// </summary>
public interface IMotionCoordinator
{
    /// <summary>
    /// True quand les animations doivent jouer normalement, false quand l'utilisateur a
    /// activé <c>ReduceMotion</c>.
    /// </summary>
    bool MotionEnabled { get; }

    /// <summary>
    /// Fait varier l'opacité jusqu'à <paramref name="opacity"/>. Si <see cref="MotionEnabled"/>
    /// est false, applique <paramref name="opacity"/> instantanément.
    /// </summary>
    Task FadeToAsync(VisualElement element, double opacity, uint duration = 250);

    /// <summary>
    /// Translation jusqu'à (<paramref name="x"/>, <paramref name="y"/>). Si <see cref="MotionEnabled"/>
    /// est false, applique la translation instantanément.
    /// </summary>
    Task TranslateToAsync(VisualElement element, double x, double y, uint duration = 250);

    /// <summary>
    /// Mise à l'échelle jusqu'à <paramref name="scale"/>. Si <see cref="MotionEnabled"/>
    /// est false, applique l'échelle instantanément.
    /// </summary>
    Task ScaleToAsync(VisualElement element, double scale, uint duration = 250);
}
