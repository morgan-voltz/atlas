using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokensDto>>;
