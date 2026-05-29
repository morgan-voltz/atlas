using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using MediatR;

namespace Atlas.Application.Veille.Rules.EvaluateFeedRules;

/// <summary>
/// Publiée par <see cref="EvaluateFeedRulesHandler"/> chaque fois qu'une règle de surveillance
/// personnalisée (F-046) a matché un ou plusieurs nouveaux items. Plusieurs handlers s'abonnent :
/// envoi d'email (si <see cref="NotifyEmail"/>), dispatch push (si <see cref="NotifyPush"/>).
/// </summary>
public sealed record FeedRuleMatchedNotification(
    UserId UserId,
    EmailAddress UserEmail,
    FeedRuleId RuleId,
    string RuleName,
    bool NotifyEmail,
    bool NotifyPush,
    IReadOnlyList<FeedRuleMatch> Matches) : INotification;
