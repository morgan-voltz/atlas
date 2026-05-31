using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.PasswordReset;

/// <summary>
/// Réinitialisation effective du mot de passe à partir du lien (userId + token) et d'un nouveau
/// mot de passe. Le token est à usage unique ; les sessions actives sont révoquées (M7).
/// </summary>
public sealed record ResetPasswordCommand(Guid UserId, string Token, string NewPassword) : IRequest<Result>;
