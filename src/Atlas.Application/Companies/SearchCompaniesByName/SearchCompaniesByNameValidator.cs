using FluentValidation;

namespace Atlas.Application.Companies.SearchCompaniesByName;

internal sealed class SearchCompaniesByNameValidator : AbstractValidator<SearchCompaniesByNameQuery>
{
    private const int MaxPageSize = 100;

    public SearchCompaniesByNameValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Term).NotEmpty().MinimumLength(2);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
