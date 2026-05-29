namespace Atlas.Maui.Theming;

/// <summary>
/// Implémentation par défaut de <see cref="IMotionCoordinator"/> branchée sur le
/// <see cref="ThemeManager"/> singleton. Lit <c>ThemeManager.ReduceMotion</c> à chaque appel
/// pour refléter immédiatement les changements de préférence sans cache à invalider.
/// </summary>
internal sealed class MotionCoordinator(ThemeManager themeManager) : IMotionCoordinator
{
    public bool MotionEnabled => !themeManager.ReduceMotion;

    public Task FadeToAsync(VisualElement element, double opacity, uint duration = 250)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (themeManager.ReduceMotion)
        {
            element.Opacity = opacity;
            return Task.CompletedTask;
        }
        return element.FadeToAsync(opacity, duration);
    }

    public Task TranslateToAsync(VisualElement element, double x, double y, uint duration = 250)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (themeManager.ReduceMotion)
        {
            element.TranslationX = x;
            element.TranslationY = y;
            return Task.CompletedTask;
        }
        return element.TranslateToAsync(x, y, duration);
    }

    public Task ScaleToAsync(VisualElement element, double scale, uint duration = 250)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (themeManager.ReduceMotion)
        {
            element.Scale = scale;
            return Task.CompletedTask;
        }
        return element.ScaleToAsync(scale, duration);
    }
}
