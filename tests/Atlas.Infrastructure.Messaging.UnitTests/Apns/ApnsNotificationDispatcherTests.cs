using System.Net;
using System.Text.Json;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Messaging.Push.Apns;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Atlas.Infrastructure.Messaging.UnitTests.Apns;

public sealed class ApnsNotificationDispatcherTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IApnsAccessTokenProvider _tokenProvider = Substitute.For<IApnsAccessTokenProvider>();
    private readonly IDeviceRegistrationRepository _devices = Substitute.For<IDeviceRegistrationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeHttpMessageHandler _handler = new();

    public ApnsNotificationDispatcherTests()
    {
        _tokenProvider.GetTokenAsync(Arg.Any<CancellationToken>()).Returns("apns-jwt-signed");
    }

    public void Dispose() => _handler.Dispose();

    private ApnsNotificationDispatcher CreateDispatcher()
    {
        var httpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://api.push.apple.com") };
        var options = Options.Create(new ApnsOptions { BundleId = "com.atlas.app" });
        return new ApnsNotificationDispatcher(
            httpClient, _tokenProvider, _devices, _unitOfWork, options,
            NullLogger<ApnsNotificationDispatcher>.Instance);
    }

    private static DeviceRegistration NewApnsDevice(UserId userId, string token, DevicePlatform platform = DevicePlatform.ApnsIos) =>
        DeviceRegistration.Register(userId, platform, token, "iPhone", Now);

    private static NotificationPayload NewPayload(string title = "Test", string body = "Body") =>
        new(title, body, new Dictionary<string, string> { ["type"] = "favorite-change", ["siren"] = "552032534" });

    [Fact]
    public async Task Dispatch_does_nothing_when_user_has_no_apns_device()
    {
        UserId userId = UserId.New();
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration>
            {
                DeviceRegistration.Register(userId, DevicePlatform.FcmAndroid, "fcm-tok", null, Now),
            });

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        _handler.Requests.Should().BeEmpty();
        await _tokenProvider.DidNotReceive().GetTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_sends_to_both_ios_and_macos_devices()
    {
        UserId userId = UserId.New();
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration>
            {
                NewApnsDevice(userId, "tok-ios"),
                NewApnsDevice(userId, "tok-mac", DevicePlatform.MacOsApns),
                DeviceRegistration.Register(userId, DevicePlatform.FcmAndroid, "tok-android", null, Now),
            });

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        _handler.Requests.Should().HaveCount(2);
        _handler.Requests.Select(r => r.RequestUri!.PathAndQuery).Should().BeEquivalentTo(
            new[] { "/3/device/tok-ios", "/3/device/tok-mac" });
    }

    [Fact]
    public async Task Dispatch_posts_with_required_headers_and_aps_payload()
    {
        UserId userId = UserId.New();
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { NewApnsDevice(userId, "tok-ios") });

        await CreateDispatcher().DispatchAsync(userId, NewPayload("Renault", "Adresse changée"), CancellationToken.None);

        _handler.Requests.Should().HaveCount(1);
        HttpRequestMessage request = _handler.Requests[0];
        request.Method.Should().Be(HttpMethod.Post);
        request.Headers.GetValues("authorization").Should().ContainSingle().Which.Should().Be("bearer apns-jwt-signed");
        request.Headers.GetValues("apns-topic").Should().ContainSingle().Which.Should().Be("com.atlas.app");
        request.Headers.GetValues("apns-push-type").Should().ContainSingle().Which.Should().Be("alert");
        request.Headers.GetValues("apns-priority").Should().ContainSingle().Which.Should().Be("10");
        request.Version.Should().Be(HttpVersion.Version20);

        string body = _handler.RequestBodies[0];
        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement aps = doc.RootElement.GetProperty("aps");
        aps.GetProperty("alert").GetProperty("title").GetString().Should().Be("Renault");
        aps.GetProperty("alert").GetProperty("body").GetString().Should().Be("Adresse changée");
        aps.GetProperty("sound").GetString().Should().Be("default");
        // Les data utilisateur sont à la racine (et pas dans aps).
        doc.RootElement.GetProperty("type").GetString().Should().Be("favorite-change");
        doc.RootElement.GetProperty("siren").GetString().Should().Be("552032534");
    }

    [Fact]
    public async Task Dispatch_removes_device_on_410_unregistered()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewApnsDevice(userId, "dead-tok");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.Gone);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_removes_device_on_400_BadDeviceToken()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewApnsDevice(userId, "dead-tok");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"reason\":\"BadDeviceToken\"}"),
        };

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_does_not_remove_device_on_transient_5xx()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewApnsDevice(userId, "tok");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.DidNotReceive().RemoveAsync(Arg.Any<DeviceRegistration>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> RequestBodies { get; } = [];

        public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            RequestBodies.Add(body);
            Requests.Add(request);
            return Responder(request);
        }
    }
}
