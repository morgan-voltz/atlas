using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Pages;

/// <summary>Destination Favoris — portefeuille d'entités suivies (doc 12 §8). Scaffold U4.0.</summary>
public sealed partial class FavorisPage : Page
{
    public FavorisPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            List.Children.Clear();
            foreach (CompanySummaryResponse company in SampleData.Companies)
            {
                List.Children.Add(new CompanySummaryCard { Company = company });
            }

            Count.Text = $"{SampleData.Companies.Count} entités suivies";
        };
    }
}
