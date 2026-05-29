using System.Globalization;
using Atlas.Application.Companies;
using Atlas.Application.Companies.GetCompanyAttachments;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Atlas.Api.Reports;

/// <summary>
/// Génère un PDF résumé d'une fiche entreprise (F-022) : identité, NAF, adresse, dirigeants,
/// liste des documents déposés au RNE. Powered by QuestPDF (community edition).
/// </summary>
public static class CompanyReportRenderer
{
    /// <summary>Active la licence community QuestPDF — à appeler une fois au démarrage de l'API.</summary>
    public static void Configure()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Render(CompanyDto company, IReadOnlyList<CompanyAttachmentDto> attachments, DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(company);
        ArgumentNullException.ThrowIfNull(attachments);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Fiche entreprise").FontSize(20).Bold();
                    col.Item().Text("Atlas — données INPI RNE").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(15);

                    col.Item().Column(identity =>
                    {
                        identity.Spacing(3);
                        identity.Item().Text(t => { t.Span("SIREN : ").Bold(); t.Span(company.Siren); });
                        identity.Item().Text(t => { t.Span("Dénomination : ").Bold(); t.Span(company.Denomination); });
                        if (!string.IsNullOrWhiteSpace(company.FormeJuridique))
                        {
                            identity.Item().Text(t => { t.Span("Forme juridique : ").Bold(); t.Span(company.FormeJuridique); });
                        }
                        if (!string.IsNullOrWhiteSpace(company.NafCode))
                        {
                            string label = string.IsNullOrWhiteSpace(company.NafLabel)
                                ? company.NafCode
                                : $"{company.NafCode} — {company.NafLabel}";
                            identity.Item().Text(t => { t.Span("Activité (NAF) : ").Bold(); t.Span(label); });
                        }
                        if (company.DateCreation is { } date)
                        {
                            identity.Item().Text(t => { t.Span("Date de création : ").Bold(); t.Span(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)); });
                        }
                    });

                    if (company.Adresse is { } address)
                    {
                        col.Item().Text("Adresse").FontSize(13).Bold();
                        col.Item().Column(addr =>
                        {
                            addr.Item().Text(address.Line ?? "(non communiquée)");
                            string locality = string.Join(' ',
                                new[] { address.PostalCode, address.City }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                            if (locality.Length > 0)
                            {
                                addr.Item().Text(locality);
                            }
                            if (!string.IsNullOrWhiteSpace(address.Country))
                            {
                                addr.Item().Text(address.Country);
                            }
                        });
                    }

                    if (company.Dirigeants.Count > 0)
                    {
                        col.Item().Text("Dirigeants").FontSize(13).Bold();
                        col.Item().Column(dir =>
                        {
                            foreach (DirigeantDto d in company.Dirigeants)
                            {
                                string line = string.IsNullOrWhiteSpace(d.Qualite)
                                    ? d.Nom
                                    : $"{d.Nom} — {d.Qualite}";
                                dir.Item().Text("• " + line);
                            }
                        });
                    }

                    col.Item().Text("Documents déposés").FontSize(13).Bold();
                    if (attachments.Count == 0)
                    {
                        col.Item().Text("Aucun document disponible.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(4);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Type").Bold();
                                h.Cell().Text("Libellé").Bold();
                                h.Cell().Text("Dépôt").Bold();
                            });

                            foreach (CompanyAttachmentDto a in attachments)
                            {
                                table.Cell().Text(a.Type);
                                table.Cell().Text(a.IsConfidential ? a.Name + " (confidentiel)" : a.Name);
                                table.Cell().Text(a.DepositedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "—");
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Généré par Atlas le ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    t.Span(generatedAt.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }
}
