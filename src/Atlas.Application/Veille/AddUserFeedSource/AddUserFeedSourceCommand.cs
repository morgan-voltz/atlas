using Atlas.Application.Veille.GetMySubscriptions;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.AddUserFeedSource;

/// <summary>
/// Ajout libre d'une source de veille par l'utilisateur (F-043). L'URL est validée (test de fetch/parsing),
/// modérée, et l'abonnement créé. Les sources sont partagées : un même flux n'est stocké qu'une fois.
/// </summary>
public sealed record AddUserFeedSourceCommand(Guid UserId, string Url, string? Name)
    : IRequest<Result<VeilleSubscriptionDto>>;
