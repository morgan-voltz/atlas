using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.DeleteFeedRule;

internal sealed class DeleteFeedRuleHandler(
    IFeedRuleRepository rules,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteFeedRuleCommand, Result>
{
    public async Task<Result> Handle(DeleteFeedRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var ruleId = new FeedRuleId(request.RuleId);

        FeedRule? rule = await rules.GetByIdAsync(ruleId, cancellationToken);
        if (rule is null)
        {
            return Result.Fail(VeilleErrors.FeedRuleNotFound);
        }

        if (!rule.UserId.Equals(userId))
        {
            return Result.Fail(VeilleErrors.FeedRuleForbidden);
        }

        await rules.RemoveAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
