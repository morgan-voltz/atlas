using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.UpdateFeedRule;

internal sealed class UpdateFeedRuleHandler(
    IFeedRuleRepository rules,
    IFeedSourceRepository sources,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFeedRuleCommand, Result<FeedRuleDto>>
{
    public async Task<Result<FeedRuleDto>> Handle(UpdateFeedRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var ruleId = new FeedRuleId(request.RuleId);

        FeedRule? rule = await rules.GetByIdAsync(ruleId, cancellationToken);
        if (rule is null)
        {
            return Result<FeedRuleDto>.Fail(VeilleErrors.FeedRuleNotFound);
        }

        if (!rule.UserId.Equals(userId))
        {
            return Result<FeedRuleDto>.Fail(VeilleErrors.FeedRuleForbidden);
        }

        FeedSourceId? sourceId = null;
        if (request.SourceId is { } rawSourceId)
        {
            sourceId = new FeedSourceId(rawSourceId);
            IReadOnlyList<FeedSource> found = await sources.GetByIdsAsync([sourceId.Value], cancellationToken);
            if (found.Count == 0)
            {
                return Result<FeedRuleDto>.Fail(VeilleErrors.InvalidFeedRule("source de veille introuvable."));
            }
        }

        Siren? mentionedSiren = null;
        if (!string.IsNullOrWhiteSpace(request.MentionedSiren))
        {
            Result<Siren> sirenResult = Siren.Create(request.MentionedSiren);
            if (sirenResult.IsFailure)
            {
                return Result<FeedRuleDto>.Fail(sirenResult.Error!);
            }
            mentionedSiren = sirenResult.Value;
        }

        Result updated = rule.Update(
            request.Name,
            request.KeywordPattern,
            sourceId,
            mentionedSiren,
            request.NotifyEmail,
            request.NotifyPush,
            request.IsActive);

        if (updated.IsFailure)
        {
            return Result<FeedRuleDto>.Fail(updated.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FeedRuleDto>.Ok(FeedRuleDto.From(rule));
    }
}
