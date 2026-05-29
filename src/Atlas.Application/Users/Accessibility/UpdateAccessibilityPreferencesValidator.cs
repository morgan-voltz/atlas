using Atlas.Domain.Users;
using FluentValidation;

namespace Atlas.Application.Users.Accessibility;

internal sealed class UpdateAccessibilityPreferencesValidator
    : AbstractValidator<UpdateAccessibilityPreferencesCommand>
{
    public UpdateAccessibilityPreferencesValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.FontPreference).IsInEnum();
    }
}
