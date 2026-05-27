using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

public sealed record DisableTwoFactorCommand(Guid UserId, string Code) : IRequest<Result>;
