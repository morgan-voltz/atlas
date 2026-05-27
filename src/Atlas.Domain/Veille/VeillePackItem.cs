namespace Atlas.Domain.Veille;

/// <summary>
/// Lien entre un <see cref="VeillePack"/> et une <see cref="FeedSource"/> partagée (type possédé par le pack).
/// Cf. doc 08 §9.x, F-042.
/// </summary>
public sealed class VeillePackItem
{
    private VeillePackItem()
    {
        // Constructeur de réhydratation EF Core.
    }

    public VeillePackItem(FeedSourceId sourceId) => SourceId = sourceId;

    public FeedSourceId SourceId { get; private set; }
}
