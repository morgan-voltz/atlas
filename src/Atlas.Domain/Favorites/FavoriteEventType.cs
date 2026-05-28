namespace Atlas.Domain.Favorites;

/// <summary>
/// Catégorie d'un <see cref="FavoriteEvent"/> (F-047 volet 2). Permet de filtrer et de styliser
/// l'affichage côté UI. Sources possibles :
/// <list type="bullet">
/// <item><see cref="RneChanged"/> — détection F-019 (snapshot RNE qui change).</item>
/// <item><see cref="BodaccPublished"/> — réservé pour F-048 (BODACC).</item>
/// </list>
/// </summary>
public enum FavoriteEventType
{
    RneChanged = 1,
    BodaccPublished = 2,
}
