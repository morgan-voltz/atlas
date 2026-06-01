using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>Destination Veille — flux éditorial des sources (doc 12 §6). Scaffold U4.0.</summary>
public sealed partial class VeillePage : Page
{
    public VeillePage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            Feed.Children.Clear();
            // Flux éditorial : on exclut les mouvements de favoris (garde-fou doc 12 §6).
            foreach (TimelineItemResponse item in SampleData.Feed)
            {
                if (item.Kind != "FavoriteEvent")
                {
                    Feed.Children.Add(new FeedEventCard { Item = item });
                }
            }
        };
    }
}
