using Atlas.Domain.Veille;
using FluentValidation;

namespace Atlas.Application.Veille.ApplyVeillePack;

internal sealed class ApplyVeillePackValidator : AbstractValidator<ApplyVeillePackCommand>
{
    public ApplyVeillePackValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.PackCode)
            .NotEmpty()
            .MaximumLength(VeillePack.MaxCodeLength);
    }
}
