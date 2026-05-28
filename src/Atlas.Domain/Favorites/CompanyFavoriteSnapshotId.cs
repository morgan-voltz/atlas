namespace Atlas.Domain.Favorites;

public readonly record struct CompanyFavoriteSnapshotId(Guid Value)
{
    public static CompanyFavoriteSnapshotId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
