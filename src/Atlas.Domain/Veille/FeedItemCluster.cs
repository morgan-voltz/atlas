using Atlas.Domain.Common;

namespace Atlas.Domain.Veille;

/// <summary>
/// Regroupement d'items similaires détectés par déduplication floue (F-045, doc 08 §9.7) : la même info
/// reprise par plusieurs sources. L'empreinte <see cref="SimHash"/> est celle de l'item « seed » (premier
/// item du cluster) ; l'appariement des items suivants se fait par distance de Hamming au seed.
/// </summary>
public sealed class FeedItemCluster : Entity<FeedItemClusterId>
{
    private FeedItemCluster()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private FeedItemCluster(
        FeedItemClusterId id,
        long simHash,
        int itemCount,
        DateTimeOffset firstPublishedAt,
        DateTimeOffset lastPublishedAt,
        DateTimeOffset createdAt)
        : base(id)
    {
        SimHash = simHash;
        ItemCount = itemCount;
        FirstPublishedAt = firstPublishedAt;
        LastPublishedAt = lastPublishedAt;
        CreatedAt = createdAt;
    }

    /// <summary>Empreinte SimHash 64 bits de l'item seed.</summary>
    public long SimHash { get; private set; }

    public int ItemCount { get; private set; }

    public DateTimeOffset FirstPublishedAt { get; private set; }

    public DateTimeOffset LastPublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static FeedItemCluster Create(long simHash, FeedItem seed, DateTimeOffset now) =>
        new(FeedItemClusterId.New(), simHash, 1, seed.PublishedAt, seed.PublishedAt, now);

    public void AddItem(FeedItem item)
    {
        ItemCount++;

        if (item.PublishedAt < FirstPublishedAt)
        {
            FirstPublishedAt = item.PublishedAt;
        }

        if (item.PublishedAt > LastPublishedAt)
        {
            LastPublishedAt = item.PublishedAt;
        }
    }
}
