using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Favorites.PollBodacc;

/// <summary>Récupère les annonces BODACC publiées récemment pour les SIREN favoris (F-048). Idempotent.</summary>
public sealed record PollBodaccForFavoritesCommand(int LookbackDays = 30, int MaxResultsPerSiren = 50)
    : IRequest<Result<BodaccPollSummary>>;

public sealed record BodaccPollSummary(int SirensQueried, int SirensFailed, int EventsCreated);
