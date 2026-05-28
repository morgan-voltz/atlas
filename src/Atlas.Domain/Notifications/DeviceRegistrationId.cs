namespace Atlas.Domain.Notifications;

public readonly record struct DeviceRegistrationId(Guid Value)
{
    public static DeviceRegistrationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
