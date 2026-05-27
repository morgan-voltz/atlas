namespace Atlas.Domain.Veille;

public readonly record struct FeedItemId(Guid Value)
{
    public static FeedItemId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
