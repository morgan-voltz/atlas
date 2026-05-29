namespace Atlas.Domain.Veille;

public readonly record struct FeedRuleId(Guid Value)
{
    public static FeedRuleId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
