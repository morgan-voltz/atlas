using System;
using Atlas.App.Models;
using Atlas.Domain.Companies;
using Atlas.Shared.Result;

namespace Atlas.App;

public sealed partial class MainPage : Page
{
    // SIREN réel de DANONE (cf. harness de test) — value object du domaine.
    private const string SampleSiren = "552032534";

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => PopulateShowcase();
    }

    /// <summary>
    /// Alimente la vitrine du kit. Conserve la preuve ADR-002 / ADR-029 (U2) : la tête Uno
    /// consomme directement le value object <c>Atlas.Domain.Siren</c> (validation Luhn), sans
    /// dépendance vers <c>Atlas.Infrastructure.*</c>. Les données affichées sont des échantillons.
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
    }
}
