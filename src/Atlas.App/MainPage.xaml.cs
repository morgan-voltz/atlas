using System;
using Atlas.App.Presentation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App;

/// <summary>
/// Coque applicative (U4) : héberge le rail (RailShell) et route chaque destination vers sa page.
/// Sur la tête WebAssembly, l'URL (hash) reflète la destination, permet le deep-link, et un léger
/// polling resynchronise la navigation sur les changements d'URL (bouton Précédent/Suivant) — doc 14 §4.
/// </summary>
public sealed partial class MainPage : Page
{
    // Polling du hash sur WASM : robuste (pas de callback JS→.NET), capte back/forward.
    private readonly DispatcherTimer _urlWatcher = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private string _currentTag = Routes.Default;

    public MainPage()
    {
        this.InitializeComponent();
        _urlWatcher.Tick += OnUrlWatcherTick;
        this.Loaded += (_, _) =>
        {
            Shell.DestinationSelected += OnDestinationSelected;

            // Deep-link : sur WASM, on démarre sur la destination de l'URL si elle est connue.
            string initial = WasmRouting.GetTag() is { } tag && Routes.IsKnown(tag) ? tag : Routes.Default;
            Shell.SelectDestination(initial);
            NavigateTo(initial);

            if (WasmRouting.IsBrowser)
            {
                _urlWatcher.Start();
            }
        };
    }

    private void OnDestinationSelected(object? sender, string tag) => NavigateTo(tag);

    // Détecte un changement d'URL externe (back/forward du navigateur) et resynchronise le rail.
    private void OnUrlWatcherTick(object? sender, object e)
    {
        if (WasmRouting.GetTag() is { } tag && Routes.IsKnown(tag) && tag != _currentTag)
        {
            Shell.SelectDestination(tag);
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        if (Routes.PageFor(tag) is not { } pageType)
        {
            return;
        }

        _currentTag = tag;

        if (Shell.NavigationFrame.CurrentSourcePageType != pageType)
        {
            Shell.NavigationFrame.Navigate(pageType);
        }

        // Reflète la destination dans l'URL (no-op hors WASM).
        WasmRouting.SetTag(tag);
    }
}
