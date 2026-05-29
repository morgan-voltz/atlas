using Atlas.Shared.Result;

namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Numéro de publication d'un brevet (F-015). Format souple : FRxxxxxxx, EPxxxxxxxxx,
/// WO/PCT, US…, avec ou sans suffixe (A1, B1…). Normalisé en majuscules sans espace.
/// </summary>
public readonly record struct PublicationNumber
{
    private const int MinLength = 4;
    private const int MaxLength = 32;

    private PublicationNumber(string value) => Value = value;

    public string Value { get; }

    public static Result<PublicationNumber> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result<PublicationNumber>.Fail(PatentErrors.InvalidPublicationNumber(raw ?? string.Empty));
        }

        string normalized = raw.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();
        if (normalized.Length is < MinLength or > MaxLength)
        {
            return Result<PublicationNumber>.Fail(PatentErrors.InvalidPublicationNumber(raw));
        }

        // Caractères acceptés : lettres, chiffres, tirets, points, slash (format PCT/WO).
        foreach (char c in normalized)
        {
            if (!char.IsLetterOrDigit(c) && c is not '-' and not '.' and not '/')
            {
                return Result<PublicationNumber>.Fail(PatentErrors.InvalidPublicationNumber(raw));
            }
        }

        return Result<PublicationNumber>.Ok(new PublicationNumber(normalized));
    }

    /// <summary>Bypass de la validation pour réhydratation depuis source contrôlée (mapping INPI).</summary>
    public static PublicationNumber FromTrustedValue(string value) => new(value);

    public override string ToString() => Value;
}
