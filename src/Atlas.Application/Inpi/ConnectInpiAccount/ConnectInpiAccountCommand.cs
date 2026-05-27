using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Inpi.ConnectInpiAccount;

public sealed record ConnectInpiAccountCommand(Guid UserId, string Username, string Password) : IRequest<Result>;
