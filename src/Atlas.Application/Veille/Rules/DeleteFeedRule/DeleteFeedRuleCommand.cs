using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Rules.DeleteFeedRule;

public sealed record DeleteFeedRuleCommand(Guid UserId, Guid RuleId) : IRequest<Result>;
