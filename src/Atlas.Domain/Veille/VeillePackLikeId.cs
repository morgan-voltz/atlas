namespace Atlas.Domain.Veille;

public readonly record struct VeillePackLikeId(Guid Value)
{
    public static VeillePackLikeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
