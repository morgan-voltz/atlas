namespace Atlas.Domain.Companies;

/// <summary>
/// Vue complète d'une unité légale telle que renvoyée par le RNE (cf. glossaire 5.2).
/// Modèle de lecture construit par l'adapter INPI ; non persisté en MVP 1.
/// </summary>
public sealed record UniteLegale(
    Siren Siren,
    string Denomination,
    string? FormeJuridique,
    Naf? ActivitePrincipale,
    Address? Adresse,
    DateOnly? DateCreation,
    bool IsDiffusible,
    IReadOnlyList<Dirigeant> Dirigeants);
