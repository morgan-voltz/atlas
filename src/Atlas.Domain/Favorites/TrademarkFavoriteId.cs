namespace Atlas.Domain.Favorites;

public readonly record struct TrademarkFavoriteId(Guid Value)
{
    public static TrademarkFavoriteId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
