using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.UpdateFeedRule;

/// <summary>Met à jour les critères, actions et l'état actif d'une règle (F-046).</summary>
public sealed record UpdateFeedRuleCommand(
    Guid UserId,
    Guid RuleId,
    string Name,
    string? KeywordPattern,
    Guid? SourceId,
    string? MentionedSiren,
    bool NotifyEmail,
    bool NotifyPush,
    bool IsActive) : IRequest<Result<FeedRuleDto>>;
