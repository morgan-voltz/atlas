using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Atlas.Infrastructure.Messaging.Push;

/// <summary>
/// <see cref="INotificationDispatcher"/> qui fan-out vers tous les <see cref="IPlatformPushDispatcher"/>
/// enregistrés (FCM, APNs, WNS…). Chaque sous-dispatcher filtre par plateforme — pas de double envoi.
/// Une exception dans un sous-dispatcher n'invalide pas les autres (isolation par try/catch + log).
/// </summary>
internal sealed class CompositeNotificationDispatcher(
    IEnumerable<IPlatformPushDispatcher> dispatchers,
    ILogger<CompositeNotificationDispatcher> logger) : INotificationDispatcher
{
    private readonly IPlatformPushDispatcher[] _dispatchers = dispatchers.ToArray();

    public Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (_dispatchers.Length == 0)
        {
            return Task.CompletedTask;
        }

        var tasks = _dispatchers
            .Select(dispatcher => DispatchSafelyAsync(dispatcher, userId, payload, ct))
            .ToArray();

        return Task.WhenAll(tasks);
    }

    private async Task DispatchSafelyAsync(
        IPlatformPushDispatcher dispatcher,
        UserId userId,
        NotificationPayload payload,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.DispatchAsync(userId, payload, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Push : échec du dispatcher {Dispatcher} pour {UserId}.",
                    dispatcher.GetType().Name,
                    userId.Value);
            }
        }
    }
}
