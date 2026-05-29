using Atlas.Domain.Notifications;
using Atlas.Domain.Users;

namespace Atlas.Infrastructure.Messaging.Push;

/// <summary>
/// Dispatcher push spécialisé pour une plateforme cible (FCM, APNs, WNS).
/// Chaque implémentation filtre les <see cref="DeviceRegistration"/> par sa propre
/// <see cref="DevicePlatform"/> et ignore les autres. Le <see cref="CompositeNotificationDispatcher"/>
/// fait le fan-out vers tous les <see cref="IPlatformPushDispatcher"/> enregistrés.
/// </summary>
internal interface IPlatformPushDispatcher
{
    Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default);
}
