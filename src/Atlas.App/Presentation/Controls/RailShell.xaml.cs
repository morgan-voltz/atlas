using System;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App.Presentation.Controls;

/// <summary>
/// Coque de navigation : rail des 5 destinations (doc 12 §3 / doc 14 §2) via NavigationView.
/// La sélection est exposée au host (qui mappe le tag → page et navigue <see cref="NavigationFrame"/>).
/// </summary>
public sealed partial class RailShell : UserControl
{
    public RailShell()
    {
        this.InitializeComponent();
    }

    /// <summary>Frame hébergeant la destination courante (<c>ContentFrame</c> du XAML).</summary>
    public Frame NavigationFrame => ContentFrame;

    /// <summary>Levé quand une destination est choisie (porte le <c>Tag</c> de l'entrée : « accueil », « recherche »…).</summary>
    public event EventHandler<string>? DestinationSelected;

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            DestinationSelected?.Invoke(this, tag);
        }
    }
}
