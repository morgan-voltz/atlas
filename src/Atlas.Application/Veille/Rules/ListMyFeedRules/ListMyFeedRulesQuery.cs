using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.ListMyFeedRules;

public sealed record ListMyFeedRulesQuery(Guid UserId) : IRequest<Result<IReadOnlyList<FeedRuleDto>>>;
