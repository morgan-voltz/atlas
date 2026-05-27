using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResultDto>>;
