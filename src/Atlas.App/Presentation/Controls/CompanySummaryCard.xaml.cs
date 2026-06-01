using Atlas.App.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Carte-aperçu d'un résultat entreprise (doc 12 §7/§12). Port XAML de CompanySummaryCard.razor.
/// Le SIREN est rendu en police mono (glyphes non ambigus, doc 16).
/// </summary>
public sealed partial class CompanySummaryCard : UserControl
{
    public CompanySummaryCard()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => Refresh();
    }

    public static readonly DependencyProperty CompanyProperty =
        DependencyProperty.Register(nameof(Company), typeof(CompanySummaryResponse), typeof(CompanySummaryCard),
            new PropertyMetadata(null, OnChanged));

    public CompanySummaryResponse? Company
    {
        get => (CompanySummaryResponse?)GetValue(CompanyProperty);
        set => SetValue(CompanyProperty, value);
    }

    public static readonly DependencyProperty SelectedProperty =
        DependencyProperty.Register(nameof(Selected), typeof(bool), typeof(CompanySummaryCard),
            new PropertyMetadata(false, OnChanged));

    /// <summary>Ligne active (panneau de droite affiché). Mise en avant fond + barre d'accent (doc 06).</summary>
    public bool Selected
    {
        get => (bool)GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((CompanySummaryCard)d).Refresh();

    private void Refresh()
    {
        VisualStateManager.GoToState(this, Selected ? "RowSelected" : "RowDefault", false);

        SubText.Inlines.Clear();
        if (Company is null)
        {
            NameText.Text = string.Empty;
            return;
        }

        NameText.Text = Company.Denomination;

        // SIREN en police mono (le Run hérite du Foreground du TextBlock, résolu par thème).
        var sirenRun = new Run { Text = FormatSiren(Company.Siren) };
        if (Application.Current.Resources.TryGetValue("AtlasFontMono", out object? mono) && mono is FontFamily monoFamily)
        {
            sirenRun.FontFamily = monoFamily;
        }

        SubText.Inlines.Add(sirenRun);

        if (!string.IsNullOrWhiteSpace(Company.Ville))
        {
            SubText.Inlines.Add(new Run { Text = $" · {Company.Ville}" });
        }

        if (!string.IsNullOrWhiteSpace(Company.NafCode))
        {
            SubText.Inlines.Add(new Run { Text = $" · NAF {Company.NafCode}" });
        }
    }

    private static string FormatSiren(string siren) =>
        siren.Length == 9 ? $"{siren[..3]} {siren[3..6]} {siren[6..]}" : siren;
}
