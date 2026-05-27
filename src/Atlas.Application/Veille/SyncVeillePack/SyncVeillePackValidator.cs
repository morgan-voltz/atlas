using Atlas.Domain.Veille;
using FluentValidation;

namespace Atlas.Application.Veille.SyncVeillePack;

internal sealed class SyncVeillePackValidator : AbstractValidator<SyncVeillePackCommand>
{
    public SyncVeillePackValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.PackCode)
            .NotEmpty()
            .MaximumLength(VeillePack.MaxCodeLength);
    }
}
