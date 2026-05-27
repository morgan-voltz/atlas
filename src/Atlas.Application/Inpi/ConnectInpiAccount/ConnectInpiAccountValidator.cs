using FluentValidation;

namespace Atlas.Application.Inpi.ConnectInpiAccount;

internal sealed class ConnectInpiAccountValidator : AbstractValidator<ConnectInpiAccountCommand>
{
    public ConnectInpiAccountValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Username).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}
