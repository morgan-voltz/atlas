using Atlas.Domain.Veille;
using FluentValidation;

namespace Atlas.Application.Veille.Rules.UpdateFeedRule;

internal sealed class UpdateFeedRuleValidator : AbstractValidator<UpdateFeedRuleCommand>
{
    public UpdateFeedRuleValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.RuleId).NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(FeedRule.MaxNameLength);

        RuleFor(command => command.KeywordPattern)
            .MaximumLength(FeedRule.MaxKeywordLength)
            .When(command => !string.IsNullOrWhiteSpace(command.KeywordPattern));

        RuleFor(command => command)
            .Must(command =>
                !string.IsNullOrWhiteSpace(command.KeywordPattern)
                || command.SourceId.HasValue
                || !string.IsNullOrWhiteSpace(command.MentionedSiren))
            .WithMessage("Au moins un critère requis (KeywordPattern, SourceId ou MentionedSiren).");

        RuleFor(command => command)
            .Must(command => command.NotifyEmail || command.NotifyPush)
            .WithMessage("Au moins une action de notification requise (NotifyEmail ou NotifyPush).");
    }
}
