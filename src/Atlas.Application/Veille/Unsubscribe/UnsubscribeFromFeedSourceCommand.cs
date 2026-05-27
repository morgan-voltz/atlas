using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Unsubscribe;

/// <summary>Suppression d'un abonnement de veille de l'utilisateur courant (F-043).</summary>
public sealed record UnsubscribeFromFeedSourceCommand(Guid UserId, Guid SubscriptionId) : IRequest<Result>;
