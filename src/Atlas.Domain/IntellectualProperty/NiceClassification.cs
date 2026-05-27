using System.Globalization;

namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Classe de la classification de Nice (1 à 45) désignant les produits/services couverts par une marque.
/// </summary>
public readonly record struct NiceClassification(int Number, string? Label)
{
    public override string ToString()
    {
        string number = Number.ToString(CultureInfo.InvariantCulture);
        return Label is null ? number : $"{number} — {Label}";
    }
}
