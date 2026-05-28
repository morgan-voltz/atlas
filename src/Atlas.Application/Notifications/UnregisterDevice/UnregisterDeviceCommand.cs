using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Notifications.UnregisterDevice;

public sealed record UnregisterDeviceCommand(Guid UserId, Guid DeviceId) : IRequest<Result>;
