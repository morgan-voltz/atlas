using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.CreateFeedRule;

internal sealed class CreateFeedRuleHandler(
    IFeedRuleRepository rules,
    IFeedSourceRepository sources,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateFeedRuleCommand, Result<FeedRuleDto>>
{
    public async Task<Result<FeedRuleDto>> Handle(CreateFeedRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

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

        Result<FeedRule> created = FeedRule.Create(
            userId,
            request.Name,
            request.KeywordPattern,
            sourceId,
            mentionedSiren,
            request.NotifyEmail,
            request.NotifyPush,
            clock.UtcNow);

        if (created.IsFailure)
        {
            return Result<FeedRuleDto>.Fail(created.Error!);
        }

        await rules.AddAsync(created.Value!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FeedRuleDto>.Ok(FeedRuleDto.From(created.Value!));
    }
}
