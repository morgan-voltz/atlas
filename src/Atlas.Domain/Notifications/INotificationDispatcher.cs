using Atlas.Domain.Users;

namespace Atlas.Domain.Notifications;

/// <summary>
/// Port de diffusion d'une notification push aux devices d'un utilisateur (F-020).
/// Le dispatcher est responsable de :
/// (a) charger les <see cref="DeviceRegistration"/> du user,
/// (b) router chacun vers le canal correspondant (FCM / APNs / WNS / …),
/// (c) gérer les tokens expirés (suppression silencieuse).
/// L'implémentation par défaut <c>LoggingNotificationDispatcher</c> log et ne pousse rien — les adapters
/// FCM/APNs/WNS arriveront en PRs séparées par plateforme.
/// </summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default);
}
