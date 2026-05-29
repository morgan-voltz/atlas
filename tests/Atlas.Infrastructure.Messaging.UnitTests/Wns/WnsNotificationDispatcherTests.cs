using System.Net;
using System.Text.Json;
using System.Xml;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Messaging.Push.Wns;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Infrastructure.Messaging.UnitTests.Wns;

public sealed class WnsNotificationDispatcherTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IWnsAccessTokenProvider _tokenProvider = Substitute.For<IWnsAccessTokenProvider>();
    private readonly IDeviceRegistrationRepository _devices = Substitute.For<IDeviceRegistrationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeHttpMessageHandler _handler = new();

    public WnsNotificationDispatcherTests()
    {
        _tokenProvider.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("WNS-BEARER");
    }

    public void Dispose() => _handler.Dispose();

    private WnsNotificationDispatcher CreateDispatcher() => new(
        new HttpClient(_handler), _tokenProvider, _devices, _unitOfWork,
        NullLogger<WnsNotificationDispatcher>.Instance);

    private static DeviceRegistration NewWnsDevice(UserId userId, string channelUri) =>
        DeviceRegistration.Register(userId, DevicePlatform.WindowsWns, channelUri, "PC bureau", Now);

    private static NotificationPayload NewPayload(string title = "Test", string body = "Body") =>
        new(title, body, new Dictionary<string, string> { ["type"] = "favorite-change", ["siren"] = "552032534" });

    [Fact]
    public async Task Dispatch_does_nothing_when_user_has_no_wns_device()
    {
        UserId userId = UserId.New();
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration>
            {
                DeviceRegistration.Register(userId, DevicePlatform.FcmAndroid, "fcm-tok", null, Now),
            });

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        _handler.Requests.Should().BeEmpty();
        await _tokenProvider.DidNotReceive().GetAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_posts_toast_xml_to_channelUri_with_bearer()
    {
        UserId userId = UserId.New();
        string channelUri = "https://db5p.notify.windows.com/?token=abc";
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { NewWnsDevice(userId, channelUri) });

        await CreateDispatcher().DispatchAsync(userId, NewPayload("Renault", "Adresse changée"), CancellationToken.None);

        _handler.Requests.Should().HaveCount(1);
        HttpRequestMessage request = _handler.Requests[0];
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsoluteUri.Should().Be(channelUri);
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization!.Parameter.Should().Be("WNS-BEARER");
        request.Headers.GetValues("X-WNS-Type").Should().ContainSingle().Which.Should().Be("wns/toast");

        string body = _handler.RequestBodies[0];
        body.Should().StartWith("<toast");
        body.Should().Contain("<text>Renault</text>");
        body.Should().Contain("<text>Adresse changée</text>");
        body.Should().Contain("template=\"ToastGeneric\"");
    }

    [Fact]
    public async Task BuildToastXml_includes_payload_data_as_json_in_launch_attribute()
    {
        var payload = new NotificationPayload(
            "Title",
            "Body",
            new Dictionary<string, string> { ["type"] = "favorite-change", ["siren"] = "552032534" });

        string xml = WnsNotificationDispatcher.BuildToastXml(payload);

        // Parse le XML et extrait l'attribut launch.
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        string launch = doc.DocumentElement!.GetAttribute("launch");
        launch.Should().NotBeEmpty();

        // launch contient le JSON sérialisé des data.
        Dictionary<string, string>? parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(launch);
        parsed.Should().NotBeNull();
        parsed!["type"].Should().Be("favorite-change");
        parsed["siren"].Should().Be("552032534");
    }

    [Fact]
    public async Task BuildToastXml_omits_launch_when_no_data()
    {
        var payload = new NotificationPayload("Title", "Body", new Dictionary<string, string>());

        string xml = WnsNotificationDispatcher.BuildToastXml(payload);

        var doc = new XmlDocument();
        doc.LoadXml(xml);
        doc.DocumentElement!.HasAttribute("launch").Should().BeFalse();
    }

    [Fact]
    public async Task Dispatch_removes_device_on_410_gone()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewWnsDevice(userId, "https://db5p.notify.windows.com/?token=dead");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.Gone);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_removes_device_on_404_not_found()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewWnsDevice(userId, "https://db5p.notify.windows.com/?token=dead");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_removes_device_when_channelUri_is_not_a_valid_absolute_uri()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewWnsDevice(userId, "not-a-uri");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
        _handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Dispatch_does_not_remove_device_on_transient_5xx()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewWnsDevice(userId, "https://db5p.notify.windows.com/?token=ok");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.DidNotReceive().RemoveAsync(Arg.Any<DeviceRegistration>(), Arg.Any<CancellationToken>());
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
