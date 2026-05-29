using System.Globalization;
using Atlas.Domain.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Veille.Rules.EvaluateFeedRules;

internal sealed class DispatchFeedRuleMatchedPushHandler(
    INotificationDispatcher dispatcher,
    ILogger<DispatchFeedRuleMatchedPushHandler> logger)
    : INotificationHandler<FeedRuleMatchedNotification>
{
    public async Task Handle(FeedRuleMatchedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!notification.NotifyPush)
        {
            return;
        }

        string title = $"Règle « {notification.RuleName} »";
        string body = notification.Matches.Count switch
        {
            0 => "Aucun item.",
            1 => $"1 nouveau item : {notification.Matches[0].Title}.",
            _ => $"{notification.Matches.Count.ToString(CultureInfo.InvariantCulture)} nouveaux items correspondent.",
        };

        var data = new Dictionary<string, string>
        {
            ["type"] = "feed-rule-matched",
            ["ruleId"] = notification.RuleId.Value.ToString(),
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
                    "Échec de dispatch push pour l'alerte F-046 règle {RuleId}.",
                    notification.RuleId.Value);
            }
        }
    }
}
