namespace Atlas.Domain.Veille;

public readonly record struct VeillePackId(Guid Value)
{
    public static VeillePackId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
