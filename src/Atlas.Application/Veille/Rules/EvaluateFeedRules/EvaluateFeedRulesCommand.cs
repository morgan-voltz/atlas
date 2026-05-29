using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.EvaluateFeedRules;

/// <summary>
/// Évalue toutes les règles actives contre les items ingérés depuis leur dernière évaluation
/// (F-046). Publie une <c>FeedRuleMatchedNotification</c> par règle déclenchée, consommée par
/// les handlers email et push. Idempotente : ré-exécutable après échec sans double-notif
/// grâce au watermark <c>LastEvaluatedAt</c> persisté par règle.
/// </summary>
public sealed record EvaluateFeedRulesCommand : IRequest<Result<FeedRuleEvaluationSummary>>;

public sealed record FeedRuleEvaluationSummary(
    int RulesProcessed,
    int RulesTriggered,
    int MatchesTotal);
