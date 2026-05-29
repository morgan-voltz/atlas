using Atlas.Domain.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Veille.Rules.EvaluateFeedRules;

internal sealed class SendFeedRuleMatchedEmailHandler(
    IEmailSender emailSender,
    ILogger<SendFeedRuleMatchedEmailHandler> logger)
    : INotificationHandler<FeedRuleMatchedNotification>
{
    public async Task Handle(FeedRuleMatchedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!notification.NotifyEmail)
        {
            return;
        }

        try
        {
            await emailSender.SendFeedRuleMatchedAsync(
                notification.UserEmail,
                notification.RuleName,
                notification.Matches,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Échec d'envoi de l'alerte F-046 par email pour la règle {RuleId}.",
                    notification.RuleId.Value);
            }
        }
    }
}
