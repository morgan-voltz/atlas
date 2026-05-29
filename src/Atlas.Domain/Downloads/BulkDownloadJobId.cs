namespace Atlas.Domain.Downloads;

public readonly record struct BulkDownloadJobId(Guid Value)
{
    public static BulkDownloadJobId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
