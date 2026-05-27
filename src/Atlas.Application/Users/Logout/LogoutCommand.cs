using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
