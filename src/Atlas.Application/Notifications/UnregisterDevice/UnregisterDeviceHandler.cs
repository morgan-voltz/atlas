using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Notifications.UnregisterDevice;

internal sealed class UnregisterDeviceHandler(
    IDeviceRegistrationRepository devices,
    IUnitOfWork unitOfWork) : IRequestHandler<UnregisterDeviceCommand, Result>
{
    public async Task<Result> Handle(UnregisterDeviceCommand request, CancellationToken cancellationToken)
    {
        DeviceRegistration? device =
            await devices.GetByIdAsync(new DeviceRegistrationId(request.DeviceId), cancellationToken);

        // Un user ne peut désenregistrer que ses propres devices.
        if (device is null || device.UserId != new UserId(request.UserId))
        {
            return Result.Fail(DeviceRegistrationErrors.NotFound);
        }

        await devices.RemoveAsync(device, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
