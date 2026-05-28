namespace Atlas.Domain.Veille;

public readonly record struct FeedItemFavoriteMatchId(Guid Value)
{
    public static FeedItemFavoriteMatchId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
