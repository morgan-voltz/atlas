using Atlas.Domain.Veille;

namespace Atlas.Application.Veille.Rules;

/// <summary>
/// DTO sortant représentant une <see cref="FeedRule"/> (F-046). Aplatit les value objects
/// (FeedSourceId, Siren) en string/Guid pour exposition HTTP.
/// </summary>
public sealed record FeedRuleDto(
    Guid Id,
    string Name,
    string? KeywordPattern,
    Guid? SourceId,
    string? MentionedSiren,
    bool NotifyEmail,
    bool NotifyPush,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastTriggeredAt,
    int TimesTriggered)
{
    public static FeedRuleDto From(FeedRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return new FeedRuleDto(
            rule.Id.Value,
            rule.Name,
            rule.KeywordPattern,
            rule.SourceId?.Value,
            rule.MentionedSiren?.Value,
            rule.NotifyEmail,
            rule.NotifyPush,
            rule.IsActive,
            rule.CreatedAt,
            rule.LastTriggeredAt,
            rule.TimesTriggered);
    }
}
