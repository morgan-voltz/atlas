using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Veille.Rules.EvaluateFeedRules;

internal sealed class EvaluateFeedRulesHandler(
    IFeedRuleRepository rulesRepository,
    IFeedItemRepository itemsRepository,
    IFeedSourceRepository sourcesRepository,
    IFeedItemFavoriteMatchRepository favoriteMatches,
    IUserRepository usersRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<EvaluateFeedRulesHandler> logger)
    : IRequestHandler<EvaluateFeedRulesCommand, Result<FeedRuleEvaluationSummary>>
{
    private const int MaxCandidateItemsPerUser = 1000;

    public async Task<Result<FeedRuleEvaluationSummary>> Handle(
        EvaluateFeedRulesCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.UtcNow;
        int rulesProcessed = 0;
        int rulesTriggered = 0;
        int matchesTotal = 0;

        IReadOnlyList<FeedRule> activeRules = await rulesRepository.ListActiveAsync(cancellationToken);
        if (activeRules.Count == 0)
        {
            return Result<FeedRuleEvaluationSummary>.Ok(new FeedRuleEvaluationSummary(0, 0, 0));
        }

        // Pré-charge tous les noms de sources qui pourraient apparaître dans les payloads de matches.
        IReadOnlyList<FeedSource> allSources = await sourcesRepository.GetActiveAsync(cancellationToken);
        Dictionary<FeedSourceId, string> sourceNameById = allSources.ToDictionary(s => s.Id, s => s.Name);

        IEnumerable<IGrouping<UserId, FeedRule>> rulesByUser = activeRules.GroupBy(r => r.UserId);

        foreach (IGrouping<UserId, FeedRule> userRules in rulesByUser)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UserId userId = userRules.Key;

            User? user = await usersRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                continue;
            }

            DateTimeOffset earliestWatermark = userRules.Min(r => r.EvaluationWatermark);
            IReadOnlyList<FeedItem> candidates = await itemsRepository.ListFetchedSinceAsync(
                earliestWatermark, MaxCandidateItemsPerUser, cancellationToken);
            if (candidates.Count == 0)
            {
                foreach (FeedRule rule in userRules)
                {
                    rule.RegisterEvaluation(now);
                    rulesProcessed++;
                }
                await unitOfWork.SaveChangesAsync(cancellationToken);
                continue;
            }

            // Si au moins une règle du user dépend de mentionedSiren, on précharge les mentions
            // pour ces items en un seul appel ; sinon on évite l'I/O.
            Dictionary<FeedItemId, IReadOnlyList<Siren>> mentionedSirensByItem = new();
            bool anyRuleNeedsMentions = userRules.Any(r => r.MentionedSiren.HasValue);
            if (anyRuleNeedsMentions)
            {
                IReadOnlyDictionary<FeedItemId, IReadOnlyList<FavoriteMention>> mentions =
                    await favoriteMatches.GetMentionsForUserAsync(
                        userId,
                        candidates.Select(c => c.Id).ToArray(),
                        cancellationToken);

                foreach ((FeedItemId itemId, IReadOnlyList<FavoriteMention> itemMentions) in mentions)
                {
                    mentionedSirensByItem[itemId] = itemMentions
                        .Select(m => Siren.FromTrustedValue(m.Siren))
                        .ToArray();
                }
            }

            foreach (FeedRule rule in userRules)
            {
                cancellationToken.ThrowIfCancellationRequested();
                rulesProcessed++;

                DateTimeOffset ruleWatermark = rule.EvaluationWatermark;
                List<FeedRuleMatch> matches = [];

                foreach (FeedItem item in candidates)
                {
                    if (item.FetchedAt <= ruleWatermark)
                    {
                        continue;
                    }

                    IReadOnlyList<Siren> mentionedSirens =
                        mentionedSirensByItem.GetValueOrDefault(item.Id) ?? [];

                    if (!rule.Matches(item, mentionedSirens))
                    {
                        continue;
                    }

                    string sourceName = sourceNameById.GetValueOrDefault(item.SourceId) ?? "(source inconnue)";
                    matches.Add(new FeedRuleMatch(
                        item.Id,
                        item.Title,
                        item.Url,
                        item.Summary,
                        sourceName,
                        item.PublishedAt));
                }

                rule.RegisterEvaluation(now);

                if (matches.Count == 0)
                {
                    continue;
                }

                rule.RegisterTrigger(now);
                rulesTriggered++;
                matchesTotal += matches.Count;

                try
                {
                    await publisher.Publish(
                        new FeedRuleMatchedNotification(
                            userId,
                            user.Email,
                            rule.Id,
                            rule.Name,
                            rule.NotifyEmail,
                            rule.NotifyPush,
                            matches),
                        cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(
                            ex,
                            "Publication de FeedRuleMatchedNotification échouée pour règle {RuleId}.",
                            rule.Id.Value);
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<FeedRuleEvaluationSummary>.Ok(new FeedRuleEvaluationSummary(
            rulesProcessed, rulesTriggered, matchesTotal));
    }
}
