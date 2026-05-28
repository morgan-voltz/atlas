using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Notifications.RegisterDevice;

public sealed record RegisterDeviceCommand(Guid UserId, string Platform, string Token, string? Label)
    : IRequest<Result<Guid>>;
