using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string Password) : IRequest<Result>;
