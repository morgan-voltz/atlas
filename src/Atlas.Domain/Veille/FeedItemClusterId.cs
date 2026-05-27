namespace Atlas.Domain.Veille;

public readonly record struct FeedItemClusterId(Guid Value)
{
    public static FeedItemClusterId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
