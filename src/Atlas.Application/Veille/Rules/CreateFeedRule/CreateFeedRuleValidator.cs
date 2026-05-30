using Atlas.Domain.Veille;
using FluentValidation;

namespace Atlas.Application.Veille.Rules.CreateFeedRule;

internal sealed class CreateFeedRuleValidator : AbstractValidator<CreateFeedRuleCommand>
{
    public CreateFeedRuleValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(FeedRule.MaxNameLength);

        RuleFor(command => command.KeywordPattern)
            .MaximumLength(FeedRule.MaxKeywordLength)
            .When(command => !string.IsNullOrWhiteSpace(command.KeywordPattern));

        // Les invariants « ≥1 critère » et « ≥1 canal de notification » sont portés par le domaine
        // (FeedRule.Create), qui renvoie le code métier `veille.invalid_feed_rule`. On ne les duplique
        // pas ici, pour ne pas masquer ce code derrière un `validation.failed` générique.
    }
}
