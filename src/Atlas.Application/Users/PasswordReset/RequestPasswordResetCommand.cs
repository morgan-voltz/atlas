using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.PasswordReset;

/// <summary>
/// Demande de réinitialisation de mot de passe (« mot de passe oublié », M7). Réponse uniforme
/// (toujours un succès) pour ne pas révéler si l'adresse existe (anti-énumération).
/// </summary>
public sealed record RequestPasswordResetCommand(string Email) : IRequest<Result>;
