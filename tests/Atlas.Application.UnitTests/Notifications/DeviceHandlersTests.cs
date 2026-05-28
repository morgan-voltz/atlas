using Atlas.Application.Notifications;
using Atlas.Application.Notifications.GetMyDevices;
using Atlas.Application.Notifications.RegisterDevice;
using Atlas.Application.Notifications.UnregisterDevice;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Notifications;

public class DeviceHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 28, 10, 0, 0, TimeSpan.Zero);

    private readonly IDeviceRegistrationRepository _devices = Substitute.For<IDeviceRegistrationRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public DeviceHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Register_fails_when_token_is_empty()
    {
        var handler = new RegisterDeviceHandler(_devices, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RegisterDeviceCommand(Guid.NewGuid(), "FcmAndroid", "", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("devices.invalid_token");
    }

    [Fact]
    public async Task Register_fails_when_platform_is_unknown()
    {
        var handler = new RegisterDeviceHandler(_devices, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RegisterDeviceCommand(Guid.NewGuid(), "BlackBerry", "token-abc", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("devices.invalid_platform");
    }

    [Fact]
    public async Task Register_persists_new_device()
    {
        Guid userId = Guid.NewGuid();
        var handler = new RegisterDeviceHandler(_devices, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RegisterDeviceCommand(userId, "FcmAndroid", "fcm-token-abc", "Pixel 8"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _devices.Received(1).AddAsync(
            Arg.Is<DeviceRegistration>(d =>
                d.UserId == new UserId(userId)
                && d.Platform == DevicePlatform.FcmAndroid
                && d.Token == "fcm-token-abc"
                && d.Label == "Pixel 8"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_touches_existing_device_when_token_already_known()
    {
        Guid userId = Guid.NewGuid();
        var existing = DeviceRegistration.Register(new UserId(userId), DevicePlatform.FcmAndroid, "fcm-token-abc", "Pixel", Now.AddDays(-1));
        _devices.GetByTokenAsync("fcm-token-abc", Arg.Any<CancellationToken>()).Returns(existing);

        var handler = new RegisterDeviceHandler(_devices, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RegisterDeviceCommand(userId, "FcmAndroid", "fcm-token-abc", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing.Id.Value);
        await _devices.DidNotReceive().AddAsync(Arg.Any<DeviceRegistration>(), Arg.Any<CancellationToken>());
        existing.LastSeenAt.Should().Be(Now);
    }

    [Fact]
    public async Task Unregister_returns_not_found_when_device_belongs_to_other_user()
    {
        Guid attackerId = Guid.NewGuid();
        Guid ownerId = Guid.NewGuid();
        var device = DeviceRegistration.Register(new UserId(ownerId), DevicePlatform.ApnsIos, "apns-token", null, Now);
        _devices.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var handler = new UnregisterDeviceHandler(_devices, _unitOfWork);

        Result result = await handler.Handle(
            new UnregisterDeviceCommand(attackerId, device.Id.Value),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("devices.not_found");
        await _devices.DidNotReceive().RemoveAsync(Arg.Any<DeviceRegistration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unregister_removes_user_device()
    {
        Guid userId = Guid.NewGuid();
        var device = DeviceRegistration.Register(new UserId(userId), DevicePlatform.WindowsWns, "wns-token", "PC bureau", Now);
        _devices.GetByIdAsync(device.Id, Arg.Any<CancellationToken>()).Returns(device);

        var handler = new UnregisterDeviceHandler(_devices, _unitOfWork);

        Result result = await handler.Handle(
            new UnregisterDeviceCommand(userId, device.Id.Value),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyDevices_returns_user_devices_ordered_by_last_seen_desc()
    {
        Guid userId = Guid.NewGuid();
        _devices.GetByUserAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration>
            {
                DeviceRegistration.Register(new UserId(userId), DevicePlatform.FcmAndroid, "tok1", "Pixel", Now.AddDays(-2)),
                DeviceRegistration.Register(new UserId(userId), DevicePlatform.ApnsIos, "tok2", "iPhone", Now),
            });

        Result<IReadOnlyList<DeviceRegistrationDto>> result = await new GetMyDevicesHandler(_devices)
            .Handle(new GetMyDevicesQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value![0].Label.Should().Be("iPhone");
        result.Value![1].Label.Should().Be("Pixel");
        // Le token ne doit jamais être renvoyé.
        typeof(DeviceRegistrationDto).GetProperty("Token").Should().BeNull();
    }
}
