using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>Destination Accueil — fil des mouvements des entités suivies (doc 12 §5). Scaffold U4.0.</summary>
public sealed partial class AccueilPage : Page
{
    public AccueilPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            Feed.Children.Clear();
            foreach (TimelineItemResponse item in SampleData.Feed)
            {
                Feed.Children.Add(new FeedEventCard { Item = item });
            }
        };
    }
}
