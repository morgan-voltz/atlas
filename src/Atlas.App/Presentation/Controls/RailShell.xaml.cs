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

    /// <summary>Sélectionne programmatiquement une destination par son tag (met à jour le rail).</summary>
    public void SelectDestination(string tag)
    {
        foreach (object item in Nav.MenuItems)
        {
            if (item is NavigationViewItem { Tag: string t } navItem && t == tag)
            {
                Nav.SelectedItem = navItem;
                return;
            }
        }
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            DestinationSelected?.Invoke(this, tag);
        }
    }
}
