using System;
using System.Collections.Generic;
using Atlas.App.Presentation.Pages;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App;

/// <summary>
/// Coque applicative (U4.0) : héberge le rail (RailShell) et route chaque destination vers sa page
/// dans le Frame interne. Le routage par URL côté WASM et l'auth seront câblés en U4.1+ (docs/15 §7).
/// </summary>
public sealed partial class MainPage : Page
{
    private static readonly Dictionary<string, Type> Destinations = new()
    {
        ["accueil"] = typeof(AccueilPage),
        ["recherche"] = typeof(RecherchePage),
        ["veille"] = typeof(VeillePage),
        ["favoris"] = typeof(FavorisPage),
        ["profil"] = typeof(ProfilPage),
    };

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            Shell.DestinationSelected += OnDestinationSelected;
            // Destination par défaut (l'entrée Accueil est sélectionnée dans le rail).
            Shell.NavigationFrame.Navigate(typeof(AccueilPage));
        };
    }

    private void OnDestinationSelected(object? sender, string tag)
    {
        if (Destinations.TryGetValue(tag, out Type? pageType) &&
            Shell.NavigationFrame.CurrentSourcePageType != pageType)
        {
            Shell.NavigationFrame.Navigate(pageType);
        }
    }
}
