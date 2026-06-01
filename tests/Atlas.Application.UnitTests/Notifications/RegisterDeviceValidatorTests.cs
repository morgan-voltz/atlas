using Atlas.Application.Notifications.RegisterDevice;
using Atlas.Domain.Notifications;
using FluentAssertions;

namespace Atlas.Application.UnitTests.Notifications;

public sealed class RegisterDeviceValidatorTests
{
    private readonly RegisterDeviceValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var command = new RegisterDeviceCommand(Guid.NewGuid(), "FcmAndroid", "token-abc", "iPhone de Morgan");

        _validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_token_fails()
    {
        var command = new RegisterDeviceCommand(Guid.NewGuid(), "FcmAndroid", "  ", null);

        _validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Token_exceeding_max_length_fails()
    {
        // Audit Lot 4 — F1 : sans validator, ce token ferait lever DeviceRegistration.Register (→ 500).
        string oversized = new('a', DeviceRegistration.MaxTokenLength + 1);
        var command = new RegisterDeviceCommand(Guid.NewGuid(), "FcmAndroid", oversized, null);

        _validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Label_exceeding_max_length_fails()
    {
        string oversizedLabel = new('x', DeviceRegistration.MaxLabelLength + 1);
        var command = new RegisterDeviceCommand(Guid.NewGuid(), "FcmAndroid", "token-abc", oversizedLabel);

        _validator.Validate(command).IsValid.Should().BeFalse();
    }
}
