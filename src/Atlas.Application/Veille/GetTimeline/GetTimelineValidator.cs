using FluentValidation;

namespace Atlas.Application.Veille.GetTimeline;

internal sealed class GetTimelineValidator : AbstractValidator<GetTimelineQuery>
{
    public GetTimelineValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Keyword)
            .MaximumLength(200)
            .When(query => query.Keyword is not null);
    }
}
