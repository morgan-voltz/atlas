using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Atlas.App.Models;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DomainSiren = Atlas.Domain.Companies.Siren;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Fiche entreprise (doc 12 §4). Port XAML de CompanyDetail.razor. Charge <c>GET /companies/{siren}</c>
/// et gère les états honnêtes (chargement / INPI non connecté / introuvable / erreur / chargée).
/// </summary>
public sealed partial class CompanyDetail : UserControl
{
    private static readonly CultureInfo Culture = new("fr-FR");

    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private ActionMode _actionMode = ActionMode.None;
    private string _loadedSiren = string.Empty;

    public CompanyDetail()
    {
        this.InitializeComponent();
    }

    /// <summary>Demande au host de naviguer vers le Profil (pour connecter INPI).</summary>
    public event EventHandler? ProfilRequested;

    public static readonly DependencyProperty SirenProperty =
        DependencyProperty.Register(nameof(Siren), typeof(string), typeof(CompanyDetail),
            new PropertyMetadata(string.Empty, OnSirenChanged));

    /// <summary>SIREN à charger (vide = rien affiché).</summary>
    public string Siren
    {
        get => (string)GetValue(SirenProperty);
        set => SetValue(SirenProperty, value);
    }

    private static async void OnSirenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var detail = (CompanyDetail)d;
        if (e.NewValue is string s && s != detail._loadedSiren)
        {
            await detail.LoadAsync(s);
        }
    }

    private async void OnMessageAction(object sender, RoutedEventArgs e)
    {
        switch (_actionMode)
        {
            case ActionMode.Retry:
                await LoadAsync(_loadedSiren);
                break;
            case ActionMode.Profil:
                ProfilRequested?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private async Task LoadAsync(string sirenValue)
    {
        _loadedSiren = sirenValue;

        if (string.IsNullOrWhiteSpace(sirenValue))
        {
            ShowOnly(null);
            return;
        }

        if (DomainSiren.Create(sirenValue) is not { IsSuccess: true } parsed)
        {
            ShowMessage("SIREN invalide", "Le SIREN doit comporter 9 chiffres avec une clé de contrôle valide.", ActionMode.None);
            return;
        }

        ShowOnly(LoadingPanel);

        Result<CompanyResponse> result = await _api.GetCompanyAsync(parsed.Value);
        if (sirenValue != _loadedSiren)
        {
            return; // un autre SIREN a été demandé entre-temps
        }

        if (result.IsSuccess)
        {
            ShowLoaded(result.Value);
            return;
        }

        switch (result.Error?.Code)
        {
            case "inpi.not_connected":
                ShowMessage("Connectez votre compte INPI", "Les fiches viennent du RNE (INPI). Renseignez vos identifiants dans le Profil.", ActionMode.Profil, "Aller au profil");
                break;
            case "companies.not_found":
                ShowMessage("Aucune entreprise pour ce SIREN", result.Error.Message, ActionMode.None);
                break;
            default:
                ShowMessage("Chargement impossible", result.Error?.Message ?? "Une erreur est survenue.", ActionMode.Retry, "Réessayer");
                break;
        }
    }

    private void ShowLoaded(CompanyResponse company)
    {
        ShowOnly(LoadedPanel);

        NameText.Text = company.Denomination;

        MetaPanel.Children.Clear();
        if (!string.IsNullOrWhiteSpace(company.FormeJuridique))
        {
            MetaPanel.Children.Add(new Chip { Text = company.FormeJuridique!, Tone = ChipTone.Info });
        }

        if (!company.IsDiffusible)
        {
            MetaPanel.Children.Add(new Chip { Text = "Diffusion restreinte", Tone = ChipTone.Neutral });
        }

        FactsPanel.Children.Clear();
        if (!string.IsNullOrWhiteSpace(company.NafCode))
        {
            string naf = string.IsNullOrWhiteSpace(company.NafLabel) ? company.NafCode! : $"{company.NafCode} — {company.NafLabel}";
            FactsPanel.Children.Add(Field("Activité (NAF)", naf));
        }

        if (company.DateCreation is { } created)
        {
            FactsPanel.Children.Add(Field("Création", created.ToString("d MMMM yyyy", Culture)));
        }

        if (FormatAddress(company.Adresse) is { } addr)
        {
            FactsPanel.Children.Add(Field("Siège", addr));
        }

        // Section Dirigeants.
        bool hasDirigeants = company.Dirigeants.Count > 0;
        DirigeantsSection.Indicator = hasDirigeants ? company.Dirigeants.Count.ToString(Culture) : null;
        DirigeantsSection.EmptyMessage = company.IsDiffusible
            ? "Aucun dirigeant publié au RNE pour cette entreprise."
            : "Diffusion restreinte : dirigeants non communiqués (INSEE).";
        DirigeantsSection.State = hasDirigeants ? SectionState.Loaded : SectionState.CoverageEmpty;
        DirigeantsSection.SectionContent = hasDirigeants ? BuildDirigeants(company.Dirigeants) : null;
    }

    private static StackPanel BuildDirigeants(IReadOnlyList<DirigeantResponse> dirigeants)
    {
        var panel = new StackPanel { Spacing = 6 };
        foreach (DirigeantResponse d in dirigeants)
        {
            var row = new StackPanel { Spacing = 1 };
            // Foreground laissé au défaut thème (lisible clair/sombre) ; le bloc vit sur une surface.
            row.Children.Add(new TextBlock { Text = d.Nom, FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            if (!string.IsNullOrWhiteSpace(d.Qualite))
            {
                row.Children.Add(new TextBlock { Text = d.Qualite, FontSize = 13, Opacity = 0.75 });
            }

            panel.Children.Add(row);
        }

        return panel;
    }

    private static LabeledField Field(string label, string value) => new() { Label = label, FieldContent = value };

    private void ShowMessage(string title, string sub, ActionMode mode, string? actionLabel = null)
    {
        ShowOnly(MessagePanel);
        MessageTitle.Text = title;
        MessageSub.Text = sub;
        _actionMode = mode;
        if (mode == ActionMode.None || actionLabel is null)
        {
            MessageAction.Visibility = Visibility.Collapsed;
        }
        else
        {
            MessageAction.Content = actionLabel;
            MessageAction.Visibility = Visibility.Visible;
        }
    }

    private void ShowOnly(FrameworkElement? panel)
    {
        LoadingPanel.Visibility = panel == LoadingPanel ? Visibility.Visible : Visibility.Collapsed;
        MessagePanel.Visibility = panel == MessagePanel ? Visibility.Visible : Visibility.Collapsed;
        LoadedPanel.Visibility = panel == LoadedPanel ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string? FormatAddress(AddressResponse? a)
    {
        if (a is null)
        {
            return null;
        }

        string cityLine = string.Join(' ', new[] { a.PostalCode, a.City }.Where(s => !string.IsNullOrWhiteSpace(s)));
        string joined = string.Join(", ", new[] { a.Line, cityLine, a.Country }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }

    private enum ActionMode
    {
        None,
        Retry,
        Profil,
    }
}
