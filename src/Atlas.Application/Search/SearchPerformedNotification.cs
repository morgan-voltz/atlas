using Atlas.Domain.Search;
using MediatR;

namespace Atlas.Application.Search;

/// <summary>
/// Émise par les handlers de recherche après un succès, pour enregistrer l'historique (F-008) de façon découplée.
/// </summary>
public sealed record SearchPerformedNotification(Guid UserId, SearchType Type, string Query) : INotification;
