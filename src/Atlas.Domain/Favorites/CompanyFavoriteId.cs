namespace Atlas.Domain.Favorites;

public readonly record struct CompanyFavoriteId(Guid Value)
{
    public static CompanyFavoriteId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
