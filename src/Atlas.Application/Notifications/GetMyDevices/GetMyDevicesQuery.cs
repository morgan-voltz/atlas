using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Notifications.GetMyDevices;

public sealed record GetMyDevicesQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<DeviceRegistrationDto>>>;
