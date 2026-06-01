using System;
using System.Collections.Generic;
using System.Linq;
using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Atlas.App.Presentation.Pages;

/// <summary>
/// Destination Recherche — champ + list-detail à 2 panneaux (doc 12 §7 / doc 14 §3). Scaffold U4.0
/// (filtre local sur des échantillons ; l'appel réel à <c>GET /companies?name=</c> arrive en U4.1).
/// </summary>
public sealed partial class RecherchePage : Page
{
    private readonly Dictionary<CompanySummaryCard, CompanySummaryResponse> _cards = new();

    public RecherchePage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) =>
        {
            ListDetail.BackRequested += (_, _) => ClearSelection();
            Render(SampleData.Companies);
        };
    }

    private void OnQueryChanged(object sender, TextChangedEventArgs e)
    {
        string q = Query.Text.Trim();
        IEnumerable<CompanySummaryResponse> matches = string.IsNullOrEmpty(q)
            ? SampleData.Companies
            : SampleData.Companies.Where(c =>
                c.Denomination.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Siren.Contains(q, StringComparison.OrdinalIgnoreCase));
        Render(matches.ToList());
    }

    private void Render(IReadOnlyList<CompanySummaryResponse> companies)
    {
        ClearSelection();
        Results.Children.Clear();
        _cards.Clear();

        foreach (CompanySummaryResponse company in companies)
        {
            var card = new CompanySummaryCard { Company = company };
            card.Tapped += OnResultTapped;
            _cards[card] = company;
            Results.Children.Add(card);
        }
    }

    private void OnResultTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not CompanySummaryCard tapped || !_cards.TryGetValue(tapped, out CompanySummaryResponse? company))
        {
            return;
        }

        foreach (CompanySummaryCard card in _cards.Keys)
        {
            card.Selected = card == tapped;
        }

        Detail.Title = company.Denomination;
        DetailField.FieldContent = company.Ville is { } ville ? $"{ville} · NAF {company.NafCode}" : $"NAF {company.NafCode}";
        ListDetail.HasSelection = true;
    }

    private void ClearSelection()
    {
        foreach (CompanySummaryCard card in _cards.Keys)
        {
            card.Selected = false;
        }

        ListDetail.HasSelection = false;
    }
}
