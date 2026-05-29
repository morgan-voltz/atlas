using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.ListMyFeedRules;

internal sealed class ListMyFeedRulesHandler(IFeedRuleRepository rules)
    : IRequestHandler<ListMyFeedRulesQuery, Result<IReadOnlyList<FeedRuleDto>>>
{
    public async Task<Result<IReadOnlyList<FeedRuleDto>>> Handle(
        ListMyFeedRulesQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        IReadOnlyList<FeedRule> rulesList = await rules.ListByUserAsync(userId, cancellationToken);
        IReadOnlyList<FeedRuleDto> dtos = rulesList.Select(FeedRuleDto.From).ToList();
        return Result<IReadOnlyList<FeedRuleDto>>.Ok(dtos);
    }
}
