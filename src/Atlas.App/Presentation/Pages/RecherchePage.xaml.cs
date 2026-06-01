using System;
using System.Collections.Generic;
using System.Threading;
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
/// Destination Recherche (doc 12 §7 / doc 14 §3). U4.1 : câblée à <c>GET /companies?name=</c> via
/// <see cref="AtlasApiClient"/> (données réelles), avec états chargement / vide / erreur (doc 12 §14).
/// </summary>
public sealed partial class RecherchePage : Page
{
    private const int MinQueryLength = 2;

    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private readonly Dictionary<CompanySummaryCard, CompanySummaryResponse> _cards = new();

    // Anti-rebond : on n'interroge l'API qu'après une courte pause de frappe — évite une requête
    // par caractère (et les 500 du backend sur des termes partiels), allège le serveur.
    private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(350) };

    private CancellationTokenSource? _cts;
    private string _lastQuery = string.Empty;

    public RecherchePage()
    {
        this.InitializeComponent();
        _debounce.Tick += OnDebounceTick;
        this.Loaded += (_, _) =>
        {
            ListDetail.BackRequested += (_, _) => ClearSelection();
            Detail.ProfilRequested += (_, _) => this.Frame?.Navigate(typeof(ProfilPage));
            ShowMessage("Tapez un nom d'entreprise pour lancer une recherche.");
        };
    }

    private void OnQueryChanged(object sender, TextChangedEventArgs e)
    {
        // Relance le minuteur à chaque frappe ; la recherche part quand la frappe se stabilise.
        _debounce.Stop();
        _debounce.Start();
    }

    private async void OnDebounceTick(object? sender, object e)
    {
        _debounce.Stop();
        await SearchAsync(Query.Text.Trim());
    }

    private async void OnRetry(object sender, RoutedEventArgs e) => await SearchAsync(_lastQuery);

    private async Task SearchAsync(string query)
    {
        _lastQuery = query;

        // Annule la requête précédente encore en vol.
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        CancellationToken ct = _cts.Token;

        if (query.Length < MinQueryLength)
        {
            ClearSelection();
            ResetCards();
            ShowMessage("Tapez un nom d'entreprise pour lancer une recherche.");
            return;
        }

        ShowLoading();

        Result<IReadOnlyList<CompanySummaryResponse>> result;
        try
        {
            result = await _api.SearchCompaniesAsync(query, ct);
        }
        catch (OperationCanceledException)
        {
            return; // Une frappe plus récente a pris le relais.
        }

        if (ct.IsCancellationRequested)
        {
            return;
        }

        if (result.IsFailure)
        {
            ShowError(result.Error?.Message ?? "La recherche a échoué.");
            return;
        }

        Render(result.Value);
    }

    private void Render(IReadOnlyList<CompanySummaryResponse> companies)
    {
        ClearSelection();
        ResetCards();

        if (companies.Count == 0)
        {
            ShowMessage($"Aucun résultat pour « {_lastQuery} ».");
            return;
        }

        HideStatus();
        foreach (CompanySummaryResponse company in companies)
        {
            var card = new CompanySummaryCard { Company = company };
            card.Tapped += OnResultTapped;
            _cards[card] = company;
            Results.Children.Add(card);
        }
    }

    private void OnResultTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not CompanySummaryCard tapped || !_cards.TryGetValue(tapped, out CompanySummaryResponse? company))
        {
            return;
        }

        foreach (CompanySummaryCard card in _cards.Keys)
        {
            card.Selected = card == tapped;
        }

        Detail.Siren = company.Siren;
        ListDetail.HasSelection = true;
    }

    // ── États (doc 12 §14) ───────────────────────────────────────────────

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
