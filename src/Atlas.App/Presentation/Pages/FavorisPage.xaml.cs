using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Destination Favoris (doc 12 §8). U4.1 : câblée à <c>GET /favorites/companies</c> (F-017) en
/// list-detail (sélection → fiche). Se rafraîchit quand un favori est retiré depuis la fiche.
/// </summary>
public sealed partial class FavorisPage : Page
{
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private readonly Dictionary<CompanySummaryCard, string> _cards = new();

    public FavorisPage()
    {
        this.InitializeComponent();
        this.Loaded += async (_, _) =>
        {
            ListDetail.BackRequested += (_, _) => ClearSelection();
            Detail.ProfilRequested += (_, _) => this.Frame?.Navigate(typeof(ProfilPage));
            Detail.FavoriteChanged += async (_, _) => await LoadAsync();
            await LoadAsync();
        };
    }

    private async void OnRetry(object sender, RoutedEventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        ShowLoading();

        Result<IReadOnlyList<CompanyFavoriteResponse>> result = await _api.GetFavoriteCompaniesAsync();
        if (result.IsFailure)
        {
            ShowError(result.Error?.Message ?? "Chargement des favoris impossible.");
            Count.Text = string.Empty;
            return;
        }

        Render(result.Value);
    }

    private void Render(IReadOnlyList<CompanyFavoriteResponse> favorites)
    {
        ResetCards();
        Count.Text = favorites.Count > 0 ? $"{favorites.Count} entités suivies" : string.Empty;

        if (favorites.Count == 0)
        {
            ClearSelection();
            ShowMessage("Aucun favori pour l'instant. Suivez une entreprise depuis sa fiche.");
            return;
        }

        HideStatus();
        foreach (CompanyFavoriteResponse fav in favorites)
        {
            var card = new CompanySummaryCard
            {
                Company = new CompanySummaryResponse(fav.Siren, fav.Name ?? fav.Siren, null, null),
            };
            card.Tapped += OnFavoriteTapped;
            _cards[card] = fav.Siren;
            Results.Children.Add(card);
        }
    }

    private void OnFavoriteTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not CompanySummaryCard tapped || !_cards.TryGetValue(tapped, out string? siren))
        {
            return;
        }

        foreach (CompanySummaryCard card in _cards.Keys)
        {
            card.Selected = card == tapped;
        }

        Detail.Siren = siren;
        ListDetail.HasSelection = true;
    }

    private void ShowLoading()
    {
        ClearSelection();
        ResetCards();
        HideStatus();
        for (int i = 0; i < 3; i++)
        {
            Results.Children.Add(new CardSkeleton());
        }
    }

    private void ShowMessage(string message)
    {
        StatusText.Text = message;
        StatusText.Visibility = Visibility.Visible;
        RetryButton.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ResetCards();
        StatusText.Text = message;
        StatusText.Visibility = Visibility.Visible;
        RetryButton.Visibility = Visibility.Visible;
    }

    private void HideStatus()
    {
        StatusText.Visibility = Visibility.Collapsed;
        RetryButton.Visibility = Visibility.Collapsed;
    }

    private void ResetCards()
    {
        Results.Children.Clear();
        _cards.Clear();
    }

    private void ClearSelection()
    {
        foreach (CompanySummaryCard card in _cards.Keys)
        {
            card.Selected = false;
        }

        ListDetail.HasSelection = false;
    }
}
