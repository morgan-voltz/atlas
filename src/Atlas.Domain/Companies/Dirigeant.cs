namespace Atlas.Domain.Companies;

/// <summary>
/// Dirigeant d'une unité légale (personne physique ou morale) et sa qualité (gérant, président, etc.).
/// </summary>
public sealed record Dirigeant(string Nom, string? Qualite);
