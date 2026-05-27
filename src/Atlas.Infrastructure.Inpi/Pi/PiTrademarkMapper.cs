using System.Globalization;
using Atlas.Domain.IntellectualProperty;
using Atlas.Infrastructure.Inpi.Pi.DTOs;

namespace Atlas.Infrastructure.Inpi.Pi;

/// <summary>
/// Mappe la réponse de recherche marques PI vers le domaine. ⚠️ Best-effort, à valider contre le schéma réel.
/// </summary>
internal static class PiTrademarkMapper
{
    public static TrademarkSummary Map(PiTrademark trademark) =>
        new(
            Denomination: string.IsNullOrWhiteSpace(trademark.Marque) ? "(dénomination non disponible)" : trademark.Marque!,
            Deposant: trademark.Deposant,
            DepositNumber: new DepositNumber(trademark.NumeroDepot ?? string.Empty),
            DateDepot: ParseDate(trademark.DateDepot),
            StatutJuridique: trademark.Statut);

    public static TrademarkDetail MapDetail(PiTrademarkNotice notice, DepositNumber requested)
    {
        IReadOnlyList<NiceClassification> classes = (notice.Classes ?? [])
            .Where(nice => nice.Numero is not null)
            .Select(nice => new NiceClassification(nice.Numero!.Value, nice.Libelle))
            .ToList();

        string? numero = string.IsNullOrWhiteSpace(notice.NumeroDepot) ? null : notice.NumeroDepot;

        return new TrademarkDetail(
            Denomination: string.IsNullOrWhiteSpace(notice.Marque) ? "(dénomination non disponible)" : notice.Marque!,
            Deposant: notice.Deposant,
            DepositNumber: numero is null ? requested : new DepositNumber(numero),
            DateDepot: ParseDate(notice.DateDepot),
            DateEnregistrement: ParseDate(notice.DateEnregistrement),
            StatutJuridique: notice.Statut,
            Type: notice.Type,
            HasImage: notice.HasImage ?? false,
            ClassesNice: classes);
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? date
            : null;
}
