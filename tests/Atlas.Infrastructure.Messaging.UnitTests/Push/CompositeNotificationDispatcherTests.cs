using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Messaging.Push;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Infrastructure.Messaging.UnitTests.Push;

public sealed class CompositeNotificationDispatcherTests
{
    private static readonly NotificationPayload Payload =
        new("Title", "Body", new Dictionary<string, string> { ["k"] = "v" });

    [Fact]
    public async Task Dispatch_fans_out_to_all_registered_platforms()
    {
        IPlatformPushDispatcher fcm = Substitute.For<IPlatformPushDispatcher>();
        IPlatformPushDispatcher apns = Substitute.For<IPlatformPushDispatcher>();
        var composite = new CompositeNotificationDispatcher(
            new[] { fcm, apns },
            NullLogger<CompositeNotificationDispatcher>.Instance);

        UserId userId = UserId.New();
        await composite.DispatchAsync(userId, Payload, CancellationToken.None);

        await fcm.Received(1).DispatchAsync(userId, Payload, Arg.Any<CancellationToken>());
        await apns.Received(1).DispatchAsync(userId, Payload, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_isolates_failures_between_platforms()
    {
        IPlatformPushDispatcher failing = Substitute.For<IPlatformPushDispatcher>();
        IPlatformPushDispatcher working = Substitute.For<IPlatformPushDispatcher>();
        failing.DispatchAsync(Arg.Any<UserId>(), Arg.Any<NotificationPayload>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new HttpRequestException("FCM down")));

        var composite = new CompositeNotificationDispatcher(
            new[] { failing, working },
            NullLogger<CompositeNotificationDispatcher>.Instance);

        UserId userId = UserId.New();

        // Le composite ne doit pas relever l'exception ; working doit avoir été appelé malgré l'échec.
        Func<Task> act = () => composite.DispatchAsync(userId, Payload, CancellationToken.None);
        await act.Should().NotThrowAsync();
        await working.Received(1).DispatchAsync(userId, Payload, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dispatch_no_op_when_no_platform_registered()
    {
        var composite = new CompositeNotificationDispatcher(
            Array.Empty<IPlatformPushDispatcher>(),
            NullLogger<CompositeNotificationDispatcher>.Instance);

        Func<Task> act = () => composite.DispatchAsync(UserId.New(), Payload, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
