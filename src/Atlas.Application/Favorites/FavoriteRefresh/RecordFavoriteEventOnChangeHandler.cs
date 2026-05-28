using System.Globalization;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Favorites.FavoriteRefresh;

/// <summary>
/// Troisième handler sur <see cref="CompanyFavoriteChangedNotification"/> (F-047 volet 2) :
/// matérialise le changement en <see cref="FavoriteEvent"/> qui apparaîtra dans la timeline
/// aux côtés des items RSS, triés chronologiquement.
/// </summary>
internal sealed class RecordFavoriteEventOnChangeHandler(
    IFavoriteEventRepository events,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    ILogger<RecordFavoriteEventOnChangeHandler> logger)
    : INotificationHandler<CompanyFavoriteChangedNotification>
{
    public async Task Handle(CompanyFavoriteChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Result<Siren> sirenResult = Siren.Create(notification.SirenValue);
        if (sirenResult.IsFailure)
        {
            // Cas impossible (la notification vient de F-019 qui a déjà validé), mais on protège.
            return;
        }

        string title = notification.Denomination is null
            ? $"Mise à jour de {notification.SirenValue}"
            : $"Mise à jour de {notification.Denomination}";

        string summary = notification.Changes.Count switch
        {
            0 => "Aucun changement (anormal).",
            1 => $"Champ modifié : {notification.Changes[0].Field}.",
            _ => $"{notification.Changes.Count.ToString(CultureInfo.InvariantCulture)} champ(s) modifié(s) : "
                + string.Join(", ", notification.Changes.Select(c => c.Field)) + ".",
        };

        try
        {
            var favoriteEvent = FavoriteEvent.Record(
                notification.UserId,
                sirenResult.Value,
                FavoriteEventType.RneChanged,
                title,
                summary,
                clock.UtcNow);

            await events.AddAsync(favoriteEvent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Échec d'enregistrement du FavoriteEvent pour {Siren}.",
                    notification.SirenValue);
            }
        }
    }
}
