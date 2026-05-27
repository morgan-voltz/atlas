using Atlas.Application.Inpi;
using Atlas.Application.Inpi.DisconnectInpiAccount;
using Atlas.Application.Inpi.GetInpiConnectionStatus;
using Atlas.Domain.Common;
using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Inpi;

public class InpiConnectionQueriesTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _credentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Status_returns_not_connected_when_no_credentials()
    {
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<InpiConnectionStatusDto> result = await new GetInpiConnectionStatusHandler(_credentials)
            .Handle(new GetInpiConnectionStatusQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Connected.Should().BeFalse();
    }

    [Fact]
    public async Task Status_returns_connected_with_details()
    {
        var credentials = InpiCredentials.Create(new UserId(Guid.NewGuid()), "enc-u", "enc-p", Now);
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(credentials);

        Result<InpiConnectionStatusDto> result = await new GetInpiConnectionStatusHandler(_credentials)
            .Handle(new GetInpiConnectionStatusQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Connected.Should().BeTrue();
        result.Value!.Status.Should().Be("Active");
        result.Value!.LastTestedAt.Should().Be(Now);
    }

    [Fact]
    public async Task Disconnect_deletes_credentials_and_saves()
    {
        Result result = await new DisconnectInpiAccountHandler(_credentials, _unitOfWork)
            .Handle(new DisconnectInpiAccountCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _credentials.Received(1).DeleteByUserAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
