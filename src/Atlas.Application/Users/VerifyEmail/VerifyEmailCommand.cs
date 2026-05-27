using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Result>;
