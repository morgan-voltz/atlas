namespace Atlas.Domain.Veille;

public readonly record struct FeedSourceId(Guid Value)
{
    public static FeedSourceId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
