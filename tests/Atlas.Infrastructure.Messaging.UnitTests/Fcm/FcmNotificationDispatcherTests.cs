using System.Net;
using System.Text.Json;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Messaging.Push.Fcm;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Atlas.Infrastructure.Messaging.UnitTests.Fcm;

public sealed class FcmNotificationDispatcherTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IFcmAccessTokenProvider _tokenProvider = Substitute.For<IFcmAccessTokenProvider>();
    private readonly IDeviceRegistrationRepository _devices = Substitute.For<IDeviceRegistrationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeHttpMessageHandler _handler = new();

    public void Dispose() => _handler.Dispose();

    public FcmNotificationDispatcherTests()
    {
        _tokenProvider.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("ya29.fake-bearer");
    }

    private FcmNotificationDispatcher CreateDispatcher()
    {
        var httpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://fcm.googleapis.com") };
        var options = Options.Create(new FcmOptions { ProjectId = "atlas-test" });
        return new FcmNotificationDispatcher(
            httpClient, _tokenProvider, _devices, _unitOfWork, options,
            NullLogger<FcmNotificationDispatcher>.Instance);
    }

    private static DeviceRegistration NewFcmDevice(UserId userId, string token = "tok-abc") =>
        DeviceRegistration.Register(userId, DevicePlatform.FcmAndroid, token, "Pixel", Now);

    [Fact]
    public async Task Dispatch_does_nothing_when_user_has_no_fcm_device()
    {
        UserId userId = UserId.New();
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration>
            {
                DeviceRegistration.Register(userId, DevicePlatform.ApnsIos, "apns-tok", null, Now),
            });

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        _handler.Requests.Should().BeEmpty();
        await _tokenProvider.DidNotReceive().GetAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_posts_message_with_bearer_token_and_correct_payload()
    {
        UserId userId = UserId.New();
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { NewFcmDevice(userId, "fcm-tok-XYZ") });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK);

        await CreateDispatcher().DispatchAsync(userId, NewPayload("Renault", "Adresse changée"), CancellationToken.None);

        _handler.Requests.Should().HaveCount(1);
        HttpRequestMessage request = _handler.Requests[0];
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.PathAndQuery.Should().Be("/v1/projects/atlas-test/messages:send");
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization!.Parameter.Should().Be("ya29.fake-bearer");

        string body = _handler.RequestBodies[0];
        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement message = doc.RootElement.GetProperty("message");
        message.GetProperty("token").GetString().Should().Be("fcm-tok-XYZ");
        message.GetProperty("notification").GetProperty("title").GetString().Should().Be("Renault");
        message.GetProperty("notification").GetProperty("body").GetString().Should().Be("Adresse changée");
        message.GetProperty("data").GetProperty("type").GetString().Should().Be("favorite-change");
    }

    [Fact]
    public async Task Dispatch_removes_device_on_404_not_found()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewFcmDevice(userId);
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_removes_device_on_400_unregistered()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewFcmDevice(userId);
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\":{\"details\":[{\"errorCode\":\"UNREGISTERED\"}]}}"),
        };

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(device, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_does_not_remove_device_on_transient_5xx()
    {
        UserId userId = UserId.New();
        DeviceRegistration device = NewFcmDevice(userId);
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { device });
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.DidNotReceive().RemoveAsync(Arg.Any<DeviceRegistration>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_handles_multiple_devices_independently()
    {
        UserId userId = UserId.New();
        DeviceRegistration good = NewFcmDevice(userId, "good-token");
        DeviceRegistration dead = NewFcmDevice(userId, "dead-token");
        _devices.GetByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DeviceRegistration> { good, dead });

        _handler.Responder = req =>
        {
            // Le body a déjà été capturé par le handler avant l'appel à Responder.
            string body = _handler.RequestBodies[^1];
            return body.Contains("dead-token", StringComparison.Ordinal)
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                : new HttpResponseMessage(HttpStatusCode.OK);
        };

        await CreateDispatcher().DispatchAsync(userId, NewPayload(), CancellationToken.None);

        await _devices.Received(1).RemoveAsync(dead, Arg.Any<CancellationToken>());
        await _devices.DidNotReceive().RemoveAsync(good, Arg.Any<CancellationToken>());
    }

    private static NotificationPayload NewPayload(string title = "Test", string body = "Body") =>
        new(title, body, new Dictionary<string, string> { ["type"] = "favorite-change", ["siren"] = "552032534" });

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
