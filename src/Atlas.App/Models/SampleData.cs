using System;
using System.Collections.Generic;

namespace Atlas.App.Models;

/// <summary>
/// Données d'échantillon pour le scaffold de navigation U4.0 (ADR-029). Purement statiques :
/// remplacées par les appels réels à l'API via <c>AtlasApiClient</c> en U4.1 (cf. docs/15 §7).
/// </summary>
internal static class SampleData
{
    public static IReadOnlyList<CompanySummaryResponse> Companies { get; } = new[]
    {
        new CompanySummaryResponse("552032534", "Danone", "Paris 9e", "70.10Z"),
        new CompanySummaryResponse("562113530", "L'Oréal", "Clichy", "70.10Z"),
        new CompanySummaryResponse("572025526", "Michelin", "Clermont-Ferrand", "22.11Z"),
        new CompanySummaryResponse("542065479", "Renault", "Boulogne-Billancourt", "29.10Z"),
    };

    public static IReadOnlyList<TimelineItemResponse> Feed { get; } = new[]
    {
        new TimelineItemResponse(
            Kind: "FavoriteEvent",
            Id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title: "Danone — changement de dirigeant (RNE)",
            Url: null,
            Summary: "Nomination d'un nouveau directeur général publiée au RNE.",
            OccurredAt: new DateTimeOffset(2026, 5, 28, 0, 0, 0, TimeSpan.Zero),
            IsRead: false,
            SourceCount: 1,
            MentionedFavorites: Array.Empty<FavoriteMentionResponse>(),
            EventSiren: "552032534"),
        new TimelineItemResponse(
            Kind: "RssItem",
            Id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title: "Le secteur agroalimentaire face à la nouvelle réglementation",
            Url: "https://example.org/article",
            Summary: "Analyse sectorielle citant plusieurs grands groupes français.",
            OccurredAt: new DateTimeOffset(2026, 5, 30, 0, 0, 0, TimeSpan.Zero),
            IsRead: true,
            SourceCount: 3,
            MentionedFavorites: new[] { new FavoriteMentionResponse("552032534", "Danone") },
            EventSiren: null),
    };
}
