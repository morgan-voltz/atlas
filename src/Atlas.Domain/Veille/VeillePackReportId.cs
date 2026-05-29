namespace Atlas.Domain.Veille;

public readonly record struct VeillePackReportId(Guid Value)
{
    public static VeillePackReportId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
