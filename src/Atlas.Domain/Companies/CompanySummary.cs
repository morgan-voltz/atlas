namespace Atlas.Domain.Companies;

/// <summary>
/// Vue allégée d'une unité légale pour les listes de résultats de recherche (cf. F-005).
/// </summary>
public sealed record CompanySummary(
    Siren Siren,
    string Denomination,
    string? Ville,
    Naf? ActivitePrincipale);
