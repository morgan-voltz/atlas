using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Users.ResendVerification;

/// <summary>
/// Renvoi du lien de vérification d'email (M7). Réponse volontairement uniforme (toujours un succès)
/// pour ne pas révéler si l'adresse existe ni si elle est déjà vérifiée (anti-énumération).
/// </summary>
public sealed record ResendVerificationCommand(string Email) : IRequest<Result>;
