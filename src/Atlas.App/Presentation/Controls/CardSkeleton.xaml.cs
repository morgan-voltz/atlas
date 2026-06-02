using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Squelette de carte-aperçu pendant le chargement d'une liste (doc 12 §11/§14).
/// Port XAML de CardSkeleton.razor (purement visuel). La pulsation d'opacité (shimmer) ne démarre
/// que si les animations système sont actives (<see cref="UISettings.AnimationsEnabled"/>) — sinon
/// le squelette reste statique (respect de <c>prefers-reduced-motion</c>, doc 06).
/// </summary>
public sealed partial class CardSkeleton : UserControl
{
    private static readonly UISettings UiSettings = new();
    private Storyboard? _shimmer;

    public CardSkeleton()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!UiSettings.AnimationsEnabled)
        {
            return;
        }

        _shimmer ??= (Storyboard)Resources["ShimmerStoryboard"];
        _shimmer.Begin();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => _shimmer?.Stop();
}
