namespace Atlas.Domain.Common;

/// <summary>
/// Marqueur pour toute entité qui accumule des <see cref="IDomainEvent"/> avant publication.
/// Implémenté par <see cref="Entity{TId}"/>. Permet à l'<c>AtlasDbContext</c> de découvrir les
/// événements sans connaître le type générique d'identifiant via une boucle sur le ChangeTracker.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
