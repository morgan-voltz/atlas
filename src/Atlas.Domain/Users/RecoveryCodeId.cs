namespace Atlas.Domain.Users;

public readonly record struct RecoveryCodeId(Guid Value)
{
    public static RecoveryCodeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
