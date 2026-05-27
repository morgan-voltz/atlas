using Atlas.Application.Inpi.ConnectInpiAccount;
using Atlas.Domain.Common;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Inpi;

public class ConnectInpiAccountHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiAuthenticationProvider _authProvider = Substitute.For<IInpiAuthenticationProvider>();
    private readonly IInpiCredentialsRepository _credentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public ConnectInpiAccountHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _crypto.Encrypt(Arg.Any<string>()).Returns(callInfo => "enc-" + callInfo.Arg<string>());
    }

    [Fact]
    public async Task Handle_with_valid_credentials_tests_then_stores_encrypted()
    {
        _authProvider.AuthenticateAsync("user@inpi.fr", "secret", Arg.Any<CancellationToken>())
            .Returns(Result<InpiSession>.Ok(new InpiSession("rne-token", Now.AddHours(1))));
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result result = await CreateHandler()
            .Handle(new ConnectInpiAccountCommand(Guid.NewGuid(), "user@inpi.fr", "secret"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _credentials.Received(1).AddAsync(
            Arg.Is<InpiCredentials>(c => c.EncryptedUsername == "enc-user@inpi.fr" && c.EncryptedPassword == "enc-secret"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_invalid_inpi_credentials_does_not_store()
    {
        _authProvider.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<InpiSession>.Fail(InpiErrors.InvalidCredentials));

        Result result = await CreateHandler()
            .Handle(new ConnectInpiAccountCommand(Guid.NewGuid(), "user@inpi.fr", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.invalid_credentials");
        await _credentials.DidNotReceive().AddAsync(Arg.Any<InpiCredentials>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_when_already_connected_updates_existing()
    {
        var existing = InpiCredentials.Create(new UserId(Guid.NewGuid()), "enc-old", "enc-old", Now);
        _authProvider.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<InpiSession>.Ok(new InpiSession("rne-token", Now.AddHours(1))));
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(existing);

        Result result = await CreateHandler()
            .Handle(new ConnectInpiAccountCommand(Guid.NewGuid(), "user@inpi.fr", "secret"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.EncryptedUsername.Should().Be("enc-user@inpi.fr");
        _credentials.Received(1).Update(existing);
        await _credentials.DidNotReceive().AddAsync(Arg.Any<InpiCredentials>(), Arg.Any<CancellationToken>());
    }

    private ConnectInpiAccountHandler CreateHandler() =>
        new(_authProvider, _credentials, _crypto, _clock, _unitOfWork);
}
