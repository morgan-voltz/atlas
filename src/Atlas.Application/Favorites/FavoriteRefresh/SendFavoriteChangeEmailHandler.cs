using Atlas.Domain.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Favorites.FavoriteRefresh;

internal sealed class SendFavoriteChangeEmailHandler(
    IEmailSender emailSender,
    ILogger<SendFavoriteChangeEmailHandler> logger)
    : INotificationHandler<CompanyFavoriteChangedNotification>
{
    public async Task Handle(CompanyFavoriteChangedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        try
        {
            await emailSender.SendFavoriteChangeAsync(
                notification.UserEmail,
                notification.SirenValue,
                notification.Denomination,
                notification.Changes,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Échec d'email n'invalide pas le cycle de refresh (autres canaux + run suivant).
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Échec d'envoi de l'alerte favori par email pour {Siren}.",
                    notification.SirenValue);
            }
        }
    }
}
