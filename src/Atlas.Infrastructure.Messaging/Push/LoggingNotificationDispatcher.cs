using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Atlas.Infrastructure.Messaging.Push;

/// <summary>
/// Implémentation par défaut de <see cref="INotificationDispatcher"/> qui log les notifications
/// au lieu de les pousser via FCM / APNs / WNS (F-020). Les adapters concrets seront livrés en
/// PRs séparées par plateforme et remplaceront cet enregistrement DI. Permet de tester F-019
/// (alertes favoris) sans dépendre des canaux push.
/// </summary>
internal sealed class LoggingNotificationDispatcher(
    IDeviceRegistrationRepository devices,
    ILogger<LoggingNotificationDispatcher> logger) : INotificationDispatcher
{
    public async Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        IReadOnlyList<DeviceRegistration> targets = await devices.GetByUserAsync(userId, ct);

        if (targets.Count == 0)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Push : aucun device enregistré pour {UserId}, notification « {Title} » ignorée.",
                    userId.Value,
                    payload.Title);
            }
            return;
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            foreach (DeviceRegistration device in targets)
            {
                logger.LogInformation(
                    "[DEV] Push simulé → {UserId} / {Platform} / {Label} : {Title} — {Body}",
                    userId.Value,
                    device.Platform,
                    device.Label ?? device.Id.ToString(),
                    payload.Title,
                    payload.Body);
            }
        }
    }
}
