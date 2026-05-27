using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

public sealed record EnableTwoFactorCommand(Guid UserId, string Code) : IRequest<Result<TwoFactorEnabledDto>>;
