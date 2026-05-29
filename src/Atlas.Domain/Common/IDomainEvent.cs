using MediatR;

namespace Atlas.Domain.Common;

/// <summary>
/// Événement domaine, publié après commit du <c>SaveChangesAsync</c> de l'<c>AtlasDbContext</c>
/// (cf. ADR-005 — outbox déférée mais sémantique « after-commit » immédiate via <see cref="IPublisher"/>).
/// L'héritage de <see cref="INotification"/> permet aux <c>INotificationHandler&lt;T&gt;</c> MediatR
/// de réagir sans wrapper supplémentaire. La dépendance reste limitée à <c>MediatR.Contracts</c>
/// (interfaces marqueur uniquement, aucune logique d'exécution).
/// </summary>
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
