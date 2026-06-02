using FluentValidation;

namespace Atlas.Application.Veille.GetTimeline;

internal sealed class GetTimelineValidator : AbstractValidator<GetTimelineQuery>
{
    public GetTimelineValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        // Pas de règle sur le curseur : un jeton illisible est traité comme « première page » (robuste).
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Keyword)
            .MaximumLength(200)
            .When(query => query.Keyword is not null);
    }
}
