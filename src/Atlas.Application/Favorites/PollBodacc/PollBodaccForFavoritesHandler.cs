using Atlas.Domain.Bodacc;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Favorites.PollBodacc;

internal sealed class PollBodaccForFavoritesHandler(
    ICompanyFavoriteRepository favorites,
    IFavoriteEventRepository events,
    IBodaccProvider bodacc,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    ILogger<PollBodaccForFavoritesHandler> logger)
    : IRequestHandler<PollBodaccForFavoritesCommand, Result<BodaccPollSummary>>
{
    public async Task<Result<BodaccPollSummary>> Handle(
        PollBodaccForFavoritesCommand request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset since = clock.UtcNow.AddDays(-request.LookbackDays);

        IReadOnlyList<CompanyFavorite> all = await favorites.GetAllAsync(cancellationToken);

        // Déduplique les appels BODACC : un SIREN suivi par plusieurs users → un seul appel API.
        var bySiren = all
            .GroupBy(f => f.Siren)
            .ToList();

        int sirensQueried = 0;
        int sirensFailed = 0;
        int eventsCreated = 0;

        foreach (IGrouping<Siren, CompanyFavorite> group in bySiren)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Siren siren = group.Key;

            Result<IReadOnlyList<BodaccAnnouncement>> announcementsResult =
                await bodacc.GetAnnouncementsAsync(siren, since, request.MaxResultsPerSiren, cancellationToken);

            if (announcementsResult.IsFailure)
            {
                sirensFailed++;
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "BODACC : échec sur SIREN {Siren} ({Code}).",
                        siren.Value,
                        announcementsResult.Error!.Code);
                }
                continue;
            }

            sirensQueried++;
            IReadOnlyList<BodaccAnnouncement> announcements = announcementsResult.Value!;
            if (announcements.Count == 0)
            {
                continue;
            }

            string[] candidateIds = announcements.Select(a => a.AnnouncementId).ToArray();

            // Pour chaque user qui suit ce SIREN, créer les events manquants.
            foreach (CompanyFavorite favorite in group)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyCollection<string> known =
                    await events.GetKnownExternalIdsAsync(favorite.UserId, candidateIds, cancellationToken);

                foreach (BodaccAnnouncement announcement in announcements)
                {
                    if (known.Contains(announcement.AnnouncementId))
                    {
                        continue;
                    }

                    string companyLabel = favorite.NameSnapshot ?? favorite.Siren.Value;
                    var favoriteEvent = FavoriteEvent.Record(
                        favorite.UserId,
                        favorite.Siren,
                        FavoriteEventType.BodaccPublished,
                        title: $"BODACC : {announcement.TypeLabel} — {companyLabel}",
                        summary: announcement.Excerpt,
                        now: announcement.PublishedAt,
                        externalId: announcement.AnnouncementId);

                    await events.AddAsync(favoriteEvent, cancellationToken);
                    eventsCreated++;
                }
            }
        }

        if (eventsCreated > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<BodaccPollSummary>.Ok(new BodaccPollSummary(sirensQueried, sirensFailed, eventsCreated));
    }
}
