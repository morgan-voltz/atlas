using System.Globalization;
using Atlas.Domain.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Favorites.FavoriteRefresh;

internal sealed class DispatchFavoriteChangePushHandler(
    INotificationDispatcher dispatcher,
    ILogger<DispatchFavoriteChangePushHandler> logger)
    : INotificationHandler<CompanyFavoriteChangedNotification>
{
    public async Task Handle(CompanyFavoriteChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string title = notification.Denomination is null
            ? $"Mise à jour de {notification.SirenValue}"
            : $"Mise à jour de {notification.Denomination}";

        string body = notification.Changes.Count switch
        {
            0 => "Aucun changement.",
            1 => $"1 changement détecté : {notification.Changes[0].Field}.",
            _ => $"{notification.Changes.Count.ToString(CultureInfo.InvariantCulture)} changements détectés.",
        };

        var data = new Dictionary<string, string>
        {
            ["type"] = "favorite-change",
            ["siren"] = notification.SirenValue,
        };

        try
        {
            await dispatcher.DispatchAsync(
                notification.UserId,
                new NotificationPayload(title, body, data),
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Échec de dispatch push pour l'alerte favori {Siren}.",
                    notification.SirenValue);
            }
        }
    }
}
