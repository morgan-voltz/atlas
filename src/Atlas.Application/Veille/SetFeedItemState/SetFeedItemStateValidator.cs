using FluentValidation;

namespace Atlas.Application.Veille.SetFeedItemState;

internal sealed class SetFeedItemStateValidator : AbstractValidator<SetFeedItemStateCommand>
{
    public SetFeedItemStateValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.FeedItemId).NotEmpty();

        // Au moins un drapeau doit être fourni, sinon la commande n'a aucun effet.
        RuleFor(command => command)
            .Must(command => command.IsRead is not null || command.IsFavorite is not null || command.IsArchived is not null)
            .WithMessage("Au moins un état (isRead, isFavorite, isArchived) doit être fourni.");
    }
}
