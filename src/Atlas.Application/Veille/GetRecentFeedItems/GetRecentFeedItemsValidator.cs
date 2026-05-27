using FluentValidation;

namespace Atlas.Application.Veille.GetRecentFeedItems;

internal sealed class GetRecentFeedItemsValidator : AbstractValidator<GetRecentFeedItemsQuery>
{
    public GetRecentFeedItemsValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
