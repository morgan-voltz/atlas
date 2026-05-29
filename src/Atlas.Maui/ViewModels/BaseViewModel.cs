using CommunityToolkit.Mvvm.ComponentModel;

namespace Atlas.Maui.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// True si <see cref="ErrorMessage"/> est non vide. Permet aux vues XAML de masquer la zone
    /// d'erreur via <c>IsVisible</c> sans converter custom (cf. accessibilité §12 : ne pas exposer
    /// au lecteur d'écran un label d'erreur vide).
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}
