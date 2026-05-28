using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Notifications.GetMyDevices;

internal sealed class GetMyDevicesHandler(IDeviceRegistrationRepository devices)
    : IRequestHandler<GetMyDevicesQuery, Result<IReadOnlyList<DeviceRegistrationDto>>>
{
    public async Task<Result<IReadOnlyList<DeviceRegistrationDto>>> Handle(
        GetMyDevicesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<DeviceRegistration> items =
            await devices.GetByUserAsync(new UserId(request.UserId), cancellationToken);

        IReadOnlyList<DeviceRegistrationDto> dto = items
            .OrderByDescending(d => d.LastSeenAt)
            .Select(d => new DeviceRegistrationDto(d.Id.Value, d.Platform.ToString(), d.Label, d.RegisteredAt, d.LastSeenAt))
            .ToList();

        return Result<IReadOnlyList<DeviceRegistrationDto>>.Ok(dto);
    }
}
