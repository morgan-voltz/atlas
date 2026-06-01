using System;
using System.Collections.Generic;
using Atlas.App.Models;
using Atlas.App.Presentation.Controls;
using Atlas.Domain.Companies;
using Atlas.Shared.Result;
using Microsoft.UI.Xaml.Input;

namespace Atlas.App;

public sealed partial class MainPage : Page
{
    // SIREN réel de DANONE (cf. harness de test) — value object du domaine.
    private const string SampleSiren = "552032534";

    private readonly Dictionary<CompanySummaryCard, CompanySummaryResponse> _results = new();

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => PopulateShowcase();
    }

    /// <summary>
    /// Alimente la vitrine du kit. Conserve la preuve ADR-002 / ADR-029 (U2) : la tête Uno
    /// consomme directement <c>Atlas.Domain.Siren</c> (validation Luhn), sans dépendance
    /// vers <c>Atlas.Infrastructure.*</c>. Les données affichées sont des échantillons.
    /// </summary>
    private void PopulateShowcase()
    {
        Result<Siren> siren = Siren.Create(SampleSiren);
        SirenField.FieldContent = siren.IsSuccess
            ? $"{siren.Value} ✓ (Luhn vérifié côté domaine)"
            : siren.Error?.Message;

        Card1.Company = new CompanySummaryResponse("552032534", "Danone", "Paris 9e", "70.10Z");
        Card2.Company = new CompanySummaryResponse("562113530", "L'Oréal", "Clichy", "70.10Z");
        Card3.Company = new CompanySummaryResponse("572025526", "Michelin", "Clermont-Ferrand", "22.11Z");

        Prov.Date = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        // List-detail : alimente la liste et câble sélection / retour.
        _results[LdCard1] = new CompanySummaryResponse("552032534", "Danone", "Paris 9e", "70.10Z");
        _results[LdCard2] = new CompanySummaryResponse("562113530", "L'Oréal", "Clichy", "70.10Z");
        _results[LdCard3] = new CompanySummaryResponse("572025526", "Michelin", "Clermont-Ferrand", "22.11Z");
        foreach ((CompanySummaryCard card, CompanySummaryResponse company) in _results)
        {
            card.Company = company;
        }

        ListDetail.BackRequested += (_, _) => ClearSelection();

        // Fil de veille : un mouvement RNE d'un favori (non lu) + un item RSS lu avec mention + dédup.
        Feed1.Item = new TimelineItemResponse(
            Kind: "FavoriteEvent",
            Id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Danone — changement de dirigeant (RNE)",
            Url: null,
            Summary: "Nomination d'un nouveau directeur général publiée au RNE.",
            OccurredAt: new DateTimeOffset(2026, 5, 28, 0, 0, 0, TimeSpan.Zero),
            IsRead: false,
            SourceCount: 1,
            MentionedFavorites: Array.Empty<FavoriteMentionResponse>(),
            EventSiren: "552032534");

        Feed2.Item = new TimelineItemResponse(
            Kind: "RssItem",
            Id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title: "Le secteur agroalimentaire face à la nouvelle réglementation",
            Url: "https://example.org/article",
            Summary: "Analyse sectorielle citant plusieurs grands groupes français.",
            OccurredAt: new DateTimeOffset(2026, 5, 30, 0, 0, 0, TimeSpan.Zero),
            IsRead: true,
            SourceCount: 3,
            MentionedFavorites: new[] { new FavoriteMentionResponse("552032534", "Danone") },
            EventSiren: null);
    }

    private void OnResultTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not CompanySummaryCard tapped || !_results.TryGetValue(tapped, out CompanySummaryResponse? company))
        {
            return;
        }

        foreach (CompanySummaryCard card in _results.Keys)
        {
            card.Selected = card == tapped;
        }

        LdDetail.Title = company.Denomination;
        LdDetailField.FieldContent = $"{company.Ville} · NAF {company.NafCode}";
        ListDetail.HasSelection = true;
    }

    private void ClearSelection()
    {
        foreach (CompanySummaryCard card in _results.Keys)
        {
            card.Selected = false;
        }

        ListDetail.HasSelection = false;
    }
}
