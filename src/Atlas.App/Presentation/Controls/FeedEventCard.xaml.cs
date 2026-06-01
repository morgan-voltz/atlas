using System;
using System.Globalization;
using Atlas.App.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Text;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Carte-aperçu « event » du fil Accueil (doc 12 §5/§12). Port XAML de FeedEventCard.razor.
/// Fait, jamais verdict (ADR-012).
/// </summary>
public sealed partial class FeedEventCard : UserControl
{
    private static readonly CultureInfo Culture = new("fr-FR");

    public FeedEventCard()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => Refresh();
    }

    /// <summary>Levé quand l'utilisateur marque l'item comme lu (porte l'identifiant de l'item).</summary>
    public event EventHandler<Guid>? MarkReadRequested;

    public static readonly DependencyProperty ItemProperty =
        DependencyProperty.Register(nameof(Item), typeof(TimelineItemResponse), typeof(FeedEventCard),
            new PropertyMetadata(null, OnChanged));

    public TimelineItemResponse? Item
    {
        get => (TimelineItemResponse?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FeedEventCard)d).Refresh();

    private bool IsFavoriteEvent =>
        string.Equals(Item?.Kind, "FavoriteEvent", StringComparison.OrdinalIgnoreCase);

    private void OnMarkRead(object sender, RoutedEventArgs e)
    {
        if (Item is { } item)
        {
            MarkReadRequested?.Invoke(this, item.Id);
        }
    }

    private void Refresh()
    {
        MentionChips.Children.Clear();
        if (Item is not { } item)
        {
            return;
        }

        bool unread = !item.IsRead;
        UnreadDot.Visibility = unread ? Visibility.Visible : Visibility.Collapsed;
        UnreadAccent.Visibility = unread ? Visibility.Visible : Visibility.Collapsed;

        SourceText.Text = (IsFavoriteEvent ? "Mouvement RNE / BODACC" : "Veille").ToUpper(Culture);
        DateText.Text = item.OccurredAt.ToString("d MMM yyyy", Culture);

        TitleText.Text = item.Title;
        TitleText.FontWeight = unread ? FontWeights.Bold : FontWeights.Normal;

        SummaryText.Text = item.Summary ?? string.Empty;
        SummaryText.Visibility = string.IsNullOrWhiteSpace(item.Summary) ? Visibility.Collapsed : Visibility.Visible;

        // Mentions de favoris (puces navigables, ton Info).
        bool hasMentions = item.MentionedFavorites is { Count: > 0 };
        MentionsPanel.Visibility = hasMentions ? Visibility.Visible : Visibility.Collapsed;
        if (hasMentions)
        {
            foreach (FavoriteMentionResponse mention in item.MentionedFavorites)
            {
                MentionChips.Children.Add(new Chip { Text = mention.Name, Tone = ChipTone.Info });
            }
        }

        // Action primaire : fiche (FavoriteEvent) ou lecture de la source (RssItem).
        if (IsFavoriteEvent && !string.IsNullOrWhiteSpace(item.EventSiren))
        {
            PrimaryAction.Content = "Voir la fiche →";
            PrimaryAction.Visibility = Visibility.Visible;
        }
        else if (!string.IsNullOrWhiteSpace(item.Url))
        {
            PrimaryAction.Content = "Lire la source ↗";
            PrimaryAction.NavigateUri = new Uri(item.Url!);
            PrimaryAction.Visibility = Visibility.Visible;
        }
        else
        {
            PrimaryAction.Visibility = Visibility.Collapsed;
        }

        DedupText.Text = item.SourceCount > 1 ? $"{item.SourceCount} sources rapportent" : string.Empty;
        DedupText.Visibility = item.SourceCount > 1 ? Visibility.Visible : Visibility.Collapsed;

        MarkReadButton.Visibility = unread ? Visibility.Visible : Visibility.Collapsed;
    }
}
