using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.TwoFactor;

public sealed record VerifyTwoFactorCommand(string ChallengeToken, string Code) : IRequest<Result<AuthTokensDto>>;
