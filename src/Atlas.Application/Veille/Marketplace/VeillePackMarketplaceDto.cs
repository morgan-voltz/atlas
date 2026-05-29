using Atlas.Domain.Veille;

namespace Atlas.Application.Veille.Marketplace;

/// <summary>
/// Vue marketplace d'un <see cref="VeillePack"/> publié par un utilisateur (F-049). Aplatit l'entité
/// pour exposition HTTP et inclut le compteur dénormalisé de likes.
/// </summary>
public sealed record VeillePackMarketplaceDto(
    string Code,
    string Name,
    string Description,
    int Version,
    int SourceCount,
    Guid AuthorUserId,
    int LikesCount,
    string Visibility,
    DateTimeOffset CreatedAt)
{
    public static VeillePackMarketplaceDto From(VeillePack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        return new VeillePackMarketplaceDto(
            pack.Code,
            pack.Name,
            pack.Description,
            pack.Version,
            pack.SourceIds.Count,
            pack.AuthorUserId?.Value ?? Guid.Empty,
            pack.LikesCount,
            pack.Visibility.ToString(),
            pack.CreatedAt);
    }
}
