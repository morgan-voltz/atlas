namespace Atlas.Domain.Common;

/// <summary>
/// Horloge système abstraite, pour rendre les use cases sensibles au temps testables de façon déterministe.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
