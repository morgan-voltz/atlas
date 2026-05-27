namespace Atlas.Domain.Companies;

/// <summary>
/// Code NAF (activité principale) et son libellé (cf. glossaire 3.4). Format : 4 chiffres + 1 lettre (ex. 6202A).
/// </summary>
public readonly record struct Naf(string Code, string? Label)
{
    public override string ToString() => Label is null ? Code : $"{Code} — {Label}";
}
