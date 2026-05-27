namespace Atlas.Domain.Veille;

public readonly record struct VeilleSubscriptionId(Guid Value)
{
    public static VeilleSubscriptionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
