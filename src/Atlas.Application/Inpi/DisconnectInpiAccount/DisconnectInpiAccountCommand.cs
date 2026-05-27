using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Inpi.DisconnectInpiAccount;

public sealed record DisconnectInpiAccountCommand(Guid UserId) : IRequest<Result>;
