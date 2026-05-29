using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

/// <summary>
/// Règle de surveillance personnalisée (F-046) — un utilisateur déclare une combinaison
/// de critères (mot-clé, source, SIREN mentionné) et reçoit une alerte (email, push)
/// chaque fois qu'un nouvel item de veille y correspond. Critères évalués en AND.
/// </summary>
public sealed class FeedRule : Entity<FeedRuleId>
{
    public const int MaxNameLength = 200;
    public const int MaxKeywordLength = 200;

    private FeedRule()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private FeedRule(
        FeedRuleId id,
        UserId userId,
        string name,
        string? keywordPattern,
        FeedSourceId? sourceId,
        Siren? mentionedSiren,
        bool notifyEmail,
        bool notifyPush,
        DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        Name = name;
        KeywordPattern = keywordPattern;
        SourceId = sourceId;
        MentionedSiren = mentionedSiren;
        NotifyEmail = notifyEmail;
        NotifyPush = notifyPush;
        IsActive = true;
        CreatedAt = createdAt;
        TimesTriggered = 0;
    }

    public UserId UserId { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Sous-chaîne (case-insensitive) à matcher sur titre OU résumé de l'item.</summary>
    public string? KeywordPattern { get; private set; }

    /// <summary>Restreint le matching aux items provenant d'une source précise.</summary>
    public FeedSourceId? SourceId { get; private set; }

    /// <summary>Restreint le matching aux items qui mentionnent une entreprise favorite (via F-047 tagging).</summary>
    public Siren? MentionedSiren { get; private set; }

    public bool NotifyEmail { get; private set; }

    public bool NotifyPush { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastEvaluatedAt { get; private set; }

    public DateTimeOffset? LastTriggeredAt { get; private set; }

    public int TimesTriggered { get; private set; }

    public static Result<FeedRule> Create(
        UserId userId,
        string name,
        string? keywordPattern,
        FeedSourceId? sourceId,
        Siren? mentionedSiren,
        bool notifyEmail,
        bool notifyPush,
        DateTimeOffset now)
    {
        Result<(string Name, string? Keyword)> validated =
            ValidateInputs(name, keywordPattern, sourceId, mentionedSiren, notifyEmail, notifyPush);
        if (validated.IsFailure)
        {
            return Result<FeedRule>.Fail(validated.Error!);
        }

        return Result<FeedRule>.Ok(new FeedRule(
            FeedRuleId.New(),
            userId,
            validated.Value.Name,
            validated.Value.Keyword,
            sourceId,
            mentionedSiren,
            notifyEmail,
            notifyPush,
            now));
    }

    public Result Update(
        string name,
        string? keywordPattern,
        FeedSourceId? sourceId,
        Siren? mentionedSiren,
        bool notifyEmail,
        bool notifyPush,
        bool isActive)
    {
        Result<(string Name, string? Keyword)> validated =
            ValidateInputs(name, keywordPattern, sourceId, mentionedSiren, notifyEmail, notifyPush);
        if (validated.IsFailure)
        {
            return Result.Fail(validated.Error!);
        }

        Name = validated.Value.Name;
        KeywordPattern = validated.Value.Keyword;
        SourceId = sourceId;
        MentionedSiren = mentionedSiren;
        NotifyEmail = notifyEmail;
        NotifyPush = notifyPush;
        IsActive = isActive;
        return Result.Ok();
    }

    /// <summary>
    /// Évalue si l'item correspond à TOUS les critères définis sur la règle (AND).
    /// <paramref name="mentionedSirens"/> = SIREN tagués sur l'item via F-047 pour cet utilisateur.
    /// </summary>
    public bool Matches(FeedItem item, IReadOnlyCollection<Siren> mentionedSirens)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(mentionedSirens);

        if (!IsActive)
        {
            return false;
        }

        if (KeywordPattern is { } pattern)
        {
            bool inTitle = item.Title.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            bool inSummary = item.Summary?.Contains(pattern, StringComparison.OrdinalIgnoreCase) ?? false;
            if (!inTitle && !inSummary)
            {
                return false;
            }
        }

        if (SourceId is { } sourceId && !item.SourceId.Equals(sourceId))
        {
            return false;
        }

        if (MentionedSiren is { } siren && !mentionedSirens.Contains(siren))
        {
            return false;
        }

        return true;
    }

    public void RegisterEvaluation(DateTimeOffset now) => LastEvaluatedAt = now;

    public void RegisterTrigger(DateTimeOffset now)
    {
        LastTriggeredAt = now;
        TimesTriggered += 1;
    }

    /// <summary>Borne basse pour ne ré-évaluer que les items nouvellement ingérés.</summary>
    public DateTimeOffset EvaluationWatermark => LastEvaluatedAt ?? CreatedAt;

    public void Deactivate() => IsActive = false;

    private static Result<(string Name, string? Keyword)> ValidateInputs(
        string name,
        string? keywordPattern,
        FeedSourceId? sourceId,
        Siren? mentionedSiren,
        bool notifyEmail,
        bool notifyPush)
    {
        string trimmedName = (name ?? string.Empty).Trim();
        if (trimmedName.Length is 0 or > MaxNameLength)
        {
            return Result<(string, string?)>.Fail(VeilleErrors.InvalidFeedRule("nom requis (≤ 200 caractères)."));
        }

        string? normalizedKeyword = keywordPattern?.Trim();
        if (string.IsNullOrEmpty(normalizedKeyword))
        {
            normalizedKeyword = null;
        }
        else if (normalizedKeyword.Length > MaxKeywordLength)
        {
            return Result<(string, string?)>.Fail(VeilleErrors.InvalidFeedRule("mot-clé ≤ 200 caractères."));
        }

        if (normalizedKeyword is null && sourceId is null && mentionedSiren is null)
        {
            return Result<(string, string?)>.Fail(VeilleErrors.InvalidFeedRule("au moins un critère requis (mot-clé, source ou SIREN)."));
        }

        if (!notifyEmail && !notifyPush)
        {
            return Result<(string, string?)>.Fail(VeilleErrors.InvalidFeedRule("au moins une action de notification requise (email ou push)."));
        }

        return Result<(string, string?)>.Ok((trimmedName, normalizedKeyword));
    }
}
