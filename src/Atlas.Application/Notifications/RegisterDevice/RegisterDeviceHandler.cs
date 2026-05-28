using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Notifications.RegisterDevice;

internal sealed class RegisterDeviceHandler(
    IDeviceRegistrationRepository devices,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterDeviceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result<Guid>.Fail(DeviceRegistrationErrors.InvalidToken);
        }

        if (!Enum.TryParse<DevicePlatform>(request.Platform, ignoreCase: true, out DevicePlatform platform))
        {
            return Result<Guid>.Fail(DeviceRegistrationErrors.InvalidPlatform(request.Platform ?? string.Empty));
        }

        var userId = new UserId(request.UserId);
        DateTimeOffset now = clock.UtcNow;

        // Upsert sur le token : si le client renvoie le même token (refresh, re-install), on touche au lieu de doublonner.
        DeviceRegistration? existing = await devices.GetByTokenAsync(request.Token.Trim(), cancellationToken);
        if (existing is not null && existing.UserId == userId)
        {
            existing.Touch(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Ok(existing.Id.Value);
        }

        var registration = DeviceRegistration.Register(userId, platform, request.Token, request.Label, now);
        await devices.AddAsync(registration, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(registration.Id.Value);
    }
}
