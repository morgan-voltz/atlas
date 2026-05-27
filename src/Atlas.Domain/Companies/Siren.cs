using Atlas.Shared.Result;

namespace Atlas.Domain.Companies;

/// <summary>
/// Identifiant d'une unité légale française : 9 chiffres avec clé de contrôle Luhn (cf. glossaire 3.1).
/// </summary>
public readonly record struct Siren
{
    private const int Length = 9;

    private Siren(string value) => Value = value;

    public string Value { get; }

    public static Result<Siren> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result<Siren>.Fail(CompanyErrors.InvalidSiren(raw ?? string.Empty));
        }

        string normalized = raw.Replace(" ", string.Empty, StringComparison.Ordinal).Trim();

        if (normalized.Length != Length || !normalized.All(char.IsAsciiDigit) || !PassesLuhn(normalized))
        {
            return Result<Siren>.Fail(CompanyErrors.InvalidSiren(raw));
        }

        return Result<Siren>.Ok(new Siren(normalized));
    }

    public override string ToString() => Value;

    private static bool PassesLuhn(string digits)
    {
        int sum = 0;
        bool doubleDigit = false;

        for (int position = digits.Length - 1; position >= 0; position--)
        {
            int current = digits[position] - '0';

            if (doubleDigit)
            {
                current *= 2;
                if (current > 9)
                {
                    current -= 9;
                }
            }

            sum += current;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }
}
