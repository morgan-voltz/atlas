namespace Atlas.Domain.Inpi;

public readonly record struct InpiCredentialsId(Guid Value)
{
    public static InpiCredentialsId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
