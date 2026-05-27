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

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? date
            : null;
}
