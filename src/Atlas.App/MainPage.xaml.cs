using System;
using Atlas.App.Presentation;
using Microsoft.UI.Xaml.Controls;

namespace Atlas.App;

/// <summary>
/// Coque applicative (U4.0/U4) : héberge le rail (RailShell) et route chaque destination vers sa page
/// dans le Frame interne. Sur la tête WebAssembly, l'URL (hash) reflète la destination et permet le
/// deep-link (doc 14 §4) via <see cref="WasmRouting"/>.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            Shell.DestinationSelected += OnDestinationSelected;

            // Deep-link : sur WASM, on démarre sur la destination de l'URL si elle est connue.
            string initial = WasmRouting.GetTag() is { } tag && Routes.IsKnown(tag) ? tag : Routes.Default;
            Shell.SelectDestination(initial);
            NavigateTo(initial);
        };
    }

    private void OnDestinationSelected(object? sender, string tag) => NavigateTo(tag);

    private void NavigateTo(string tag)
    {
        if (Routes.PageFor(tag) is not { } pageType)
        {
            return;
        }

        if (Shell.NavigationFrame.CurrentSourcePageType != pageType)
        {
            Shell.NavigationFrame.Navigate(pageType);
        }

        // Reflète la destination dans l'URL (no-op hors WASM).
        WasmRouting.SetTag(tag);
    }
}
