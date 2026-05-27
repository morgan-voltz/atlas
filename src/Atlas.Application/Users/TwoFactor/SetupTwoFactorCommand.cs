using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

public sealed record SetupTwoFactorCommand(Guid UserId) : IRequest<Result<TwoFactorSetupDto>>;
