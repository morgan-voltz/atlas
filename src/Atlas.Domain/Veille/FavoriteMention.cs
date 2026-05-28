namespace Atlas.Domain.Veille;

/// <summary>
/// Référence à une entreprise favorite d'un utilisateur mentionnée dans un item de la timeline (F-047).
/// </summary>
public sealed record FavoriteMention(string Siren, string Name);
