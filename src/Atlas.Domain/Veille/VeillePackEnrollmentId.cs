namespace Atlas.Domain.Veille;

public readonly record struct VeillePackEnrollmentId(Guid Value)
{
    public static VeillePackEnrollmentId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
