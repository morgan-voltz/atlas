using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Atlas.App.Services;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Destination Veille — flux éditorial des sources (doc 12 §6). U4.1 : câblé à
/// <c>GET /feed/timeline?editorialOnly=true</c> (F-047), marquage lu optimiste.
/// </summary>
public sealed partial class VeillePage : Page
{
    private readonly AtlasApiClient _api = AppServices.Get<AtlasApiClient>();
    private readonly Dictionary<Guid, FeedEventCard> _cards = new();

    public VeillePage()
    {
        this.InitializeComponent();
        this.Loaded += async (_, _) => await LoadAsync();
    }

    private async void OnRetry(object sender, RoutedEventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        ShowLoading();
        Result<IReadOnlyList<TimelineItemResponse>> result = await _api.GetVeilleFeedAsync();
        if (result.IsFailure)
        {
            ShowError(result.Error?.Message ?? "Chargement de la veille impossible.");
            return;
        }

        Render(result.Value);
    }

    private void Render(IReadOnlyList<TimelineItemResponse> items)
    {
        Reset();
        if (items.Count == 0)
        {
            ShowMessage("Aucune actualité pour l'instant. Abonnez-vous à des sources de veille.");
            return;
        }

        HideStatus();
        foreach (TimelineItemResponse item in items)
        {
            var card = new FeedEventCard { Item = item };
            card.MarkReadRequested += OnMarkRead;
            _cards[item.Id] = card;
            Feed.Children.Add(card);
        }
    }

    private async void OnMarkRead(object? sender, Guid id)
    {
        Result result = await _api.MarkFeedItemReadAsync(id);
        if (result.IsSuccess && _cards.TryGetValue(id, out FeedEventCard? card) && card.Item is { } item)
        {
            card.Item = item with { IsRead = true };
        }
    }

    private void ShowLoading()
    {
        Reset();
        HideStatus();
        for (int i = 0; i < 2; i++)
        {
            Feed.Children.Add(new CardSkeleton());
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
        Reset();
        StatusText.Text = message;
        StatusText.Visibility = Visibility.Visible;
        RetryButton.Visibility = Visibility.Visible;
    }

    private void HideStatus()
    {
        StatusText.Visibility = Visibility.Collapsed;
        RetryButton.Visibility = Visibility.Collapsed;
    }

    private void Reset()
    {
        Feed.Children.Clear();
        _cards.Clear();
    }
}
