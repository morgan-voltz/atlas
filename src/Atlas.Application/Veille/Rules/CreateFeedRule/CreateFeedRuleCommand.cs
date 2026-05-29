using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.CreateFeedRule;

/// <summary>
/// Crée une règle de surveillance personnalisée pour l'utilisateur (F-046).
/// Au moins un critère (Keyword / SourceId / MentionedSiren) et au moins une action
/// (NotifyEmail / NotifyPush) doivent être renseignés.
/// </summary>
public sealed record CreateFeedRuleCommand(
    Guid UserId,
    string Name,
    string? KeywordPattern,
    Guid? SourceId,
    string? MentionedSiren,
    bool NotifyEmail,
    bool NotifyPush) : IRequest<Result<FeedRuleDto>>;
