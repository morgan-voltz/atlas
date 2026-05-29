namespace Atlas.Domain.Favorites;

public readonly record struct PatentFavoriteId(Guid Value)
{
    public static PatentFavoriteId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
