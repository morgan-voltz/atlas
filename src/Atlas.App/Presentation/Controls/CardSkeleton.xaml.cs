using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Squelette de carte-aperçu pendant le chargement d'une liste (doc 12 §11/§14).
/// Port XAML de CardSkeleton.razor (purement visuel).
/// </summary>
public sealed partial class CardSkeleton : UserControl
{
    public CardSkeleton()
    {
        this.InitializeComponent();
    }
}
