using Atlas.Domain.Veille;
using FluentValidation;

namespace Atlas.Application.Veille.Marketplace.CreateUserVeillePack;

internal sealed class CreateUserVeillePackValidator : AbstractValidator<CreateUserVeillePackCommand>
{
    public CreateUserVeillePackValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(VeillePack.MaxCodeLength);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(VeillePack.MaxNameLength);

        RuleFor(command => command.Description)
            .MaximumLength(VeillePack.MaxDescriptionLength);

        RuleFor(command => command.SubscriptionIds)
            .NotEmpty()
            .WithMessage("Au moins un abonnement doit être inclus dans le pack.");
    }
}
