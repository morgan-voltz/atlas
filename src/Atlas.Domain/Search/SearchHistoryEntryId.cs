namespace Atlas.Domain.Search;

public readonly record struct SearchHistoryEntryId(Guid Value)
{
    public static SearchHistoryEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
