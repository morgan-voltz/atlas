using System.Text;
using Atlas.Api.Reports;
using Atlas.Application.Companies;
using Atlas.Application.Companies.GetCompanyAttachments;
using Atlas.Domain.Companies.Attachments;
using FluentAssertions;

namespace Atlas.Api.IntegrationTests.Reports;

/// <summary>
/// Smoke tests F-022 : on vérifie que le renderer PDF n'explose pas avec divers payloads
/// (vide / complet / bilans confidentiels) et qu'il produit un PDF binaire valide
/// (magic number "%PDF" en tête).
/// </summary>
public sealed class CompanyReportRendererTests
{
    private static readonly DateTimeOffset GeneratedAt = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    static CompanyReportRendererTests() => CompanyReportRenderer.Configure();

    [Fact]
    public void Render_with_minimal_company_produces_valid_pdf()
    {
        var company = new CompanyDto(
            Siren: "552032534",
            Denomination: "Renault",
            FormeJuridique: null,
            NafCode: null,
            NafLabel: null,
            Adresse: null,
            DateCreation: null,
            IsDiffusible: true,
            Dirigeants: Array.Empty<DirigeantDto>());

        byte[] pdf = CompanyReportRenderer.Render(company, Array.Empty<CompanyAttachmentDto>(), GeneratedAt);

        pdf.Should().NotBeNull();
        pdf.Length.Should().BeGreaterThan(100, "un PDF même minimal contient plus que quelques octets");

        // Signature PDF.
        Encoding.ASCII.GetString(pdf, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public void Render_with_complete_company_and_attachments_produces_valid_pdf()
    {
        var company = new CompanyDto(
            Siren: "552032534",
            Denomination: "Renault SA",
            FormeJuridique: "SA",
            NafCode: "29.10Z",
            NafLabel: "Construction de véhicules automobiles",
            Adresse: new AddressDto("13-15 quai Le Gallo", "92100", "Boulogne-Billancourt", "France"),
            DateCreation: new DateOnly(1899, 2, 25),
            IsDiffusible: true,
            Dirigeants: new[]
            {
                new DirigeantDto("Luca de Meo", "Directeur général"),
                new DirigeantDto("Jean-Dominique Senard", "Président"),
            });

        var attachments = new List<CompanyAttachmentDto>
        {
            new("a1", "Acte", "Statuts à jour", new DateOnly(2024, 1, 15), 12345, false),
            new("b1", "Bilan", "Comptes 2024", new DateOnly(2025, 6, 30), 67890, true),
        };

        byte[] pdf = CompanyReportRenderer.Render(company, attachments, GeneratedAt);

        Encoding.ASCII.GetString(pdf, 0, 4).Should().Be("%PDF");
        pdf.Length.Should().BeGreaterThan(500);
    }

    [Fact]
    public void Render_handles_empty_attachments_gracefully()
    {
        var company = new CompanyDto(
            "552032534", "Renault", "SA", "29.10Z", "Construction de véhicules", null, null, true,
            Array.Empty<DirigeantDto>());

        Action act = () => CompanyReportRenderer.Render(company, Array.Empty<CompanyAttachmentDto>(), GeneratedAt);

        act.Should().NotThrow();
    }
}
