using FluentValidation;

namespace Atlas.Application.IntellectualProperty.SearchTrademarksByName;

internal sealed class SearchTrademarksByNameValidator : AbstractValidator<SearchTrademarksByNameQuery>
{
    private const int MaxPageSize = 100;

    public SearchTrademarksByNameValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Term).NotEmpty().MinimumLength(2);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
