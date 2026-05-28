namespace Atlas.Domain.Favorites;

public readonly record struct FavoriteEventId(Guid Value)
{
    public static FavoriteEventId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
