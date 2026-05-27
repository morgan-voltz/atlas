using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Privacy;

public sealed record DeleteAccountCommand(Guid UserId) : IRequest<Result>;
