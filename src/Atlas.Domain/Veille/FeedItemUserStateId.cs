namespace Atlas.Domain.Veille;

public readonly record struct FeedItemUserStateId(Guid Value)
{
    public static FeedItemUserStateId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
