using System.Globalization;
using Atlas.Domain.IntellectualProperty;
using Atlas.Infrastructure.Inpi.Pi.DTOs;

namespace Atlas.Infrastructure.Inpi.Pi;

/// <summary>Mapping défensif PI brevets → domaine (F-015). Tous les champs sont optionnels.</summary>
internal static class PiPatentMapper
{
    public static PatentDetail Map(PiPatentNotice notice, PublicationNumber fallback)
    {
        ArgumentNullException.ThrowIfNull(notice);

        PublicationNumber number = string.IsNullOrWhiteSpace(notice.NumeroPublication)
            ? fallback
            : PublicationNumber.FromTrustedValue(notice.NumeroPublication.Trim().ToUpperInvariant());

        string title = string.IsNullOrWhiteSpace(notice.Titre) ? "(sans titre)" : notice.Titre.Trim();
        string? applicant = string.IsNullOrWhiteSpace(notice.Deposant) ? null : notice.Deposant.Trim();
        IReadOnlyList<string> inventors = (notice.Inventeurs ?? [])
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .ToList();

        return new PatentDetail(
            number,
            title,
            applicant,
            inventors,
            TryParseDate(notice.DateDepot),
            TryParseDate(notice.DatePublication),
            string.IsNullOrWhiteSpace(notice.Statut) ? null : notice.Statut.Trim(),
            string.IsNullOrWhiteSpace(notice.Abrege) ? null : notice.Abrege.Trim());
    }

    private static DateOnly? TryParseDate(string? raw) =>
        DateOnly.TryParse(raw, CultureInfo.InvariantCulture, out DateOnly value) ? value : null;
}
