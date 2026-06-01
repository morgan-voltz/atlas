using System;
using System.Collections.Generic;
using Atlas.App.Presentation.Pages;

namespace Atlas.App.Presentation;

/// <summary>
/// Table des destinations : tag du rail ↔ page ↔ segment d'URL (doc 14 §4). Le tag sert aussi de
/// segment d'URL (#/recherche). Source unique pour la coque (RailShell) et la synchro d'URL WASM.
/// </summary>
internal static class Routes
{
    public const string Default = "accueil";

    private static readonly Dictionary<string, Type> TagToPage = new()
    {
        ["accueil"] = typeof(AccueilPage),
        ["recherche"] = typeof(RecherchePage),
        ["veille"] = typeof(VeillePage),
        ["favoris"] = typeof(FavorisPage),
        ["profil"] = typeof(ProfilPage),
    };

    public static Type? PageFor(string tag) => TagToPage.TryGetValue(tag, out Type? t) ? t : null;

    public static bool IsKnown(string tag) => TagToPage.ContainsKey(tag);
}
