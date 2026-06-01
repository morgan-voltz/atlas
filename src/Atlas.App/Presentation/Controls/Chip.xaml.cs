using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Controls;

/// <summary>Ton visuel d'une <see cref="Chip"/> — renforce le sens, jamais porteur unique (doc 06).</summary>
public enum ChipTone
{
    /// <summary>Contour discret (ex. « Diffusion restreinte », statut neutre).</summary>
    Neutral,

    /// <summary>Teinte primaire pleine (ex. forme juridique, mention de favori).</summary>
    Info,

    /// <summary>Teinte primaire + bordure (ex. statut « Connecté »).</summary>
    Ok,

    /// <summary>Teinte d'erreur (ex. statut « À reconnecter »).</summary>
    Warn,
}

/// <summary>
/// Atome puce/badge (doc 12 §11). Port XAML de Chip.razor. Le <see cref="Text"/> est toujours
/// présent ; le <see cref="Tone"/> ne fait que renforcer (doc 06).
/// </summary>
public sealed partial class Chip : UserControl
{
    public Chip()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => ApplyTone();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Chip), new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty ToneProperty =
        DependencyProperty.Register(nameof(Tone), typeof(ChipTone), typeof(Chip),
            new PropertyMetadata(ChipTone.Neutral, OnToneChanged));

    public ChipTone Tone
    {
        get => (ChipTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private static void OnToneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Chip)d).ApplyTone();

    private void ApplyTone() => VisualStateManager.GoToState(this, Tone.ToString(), false);
}
