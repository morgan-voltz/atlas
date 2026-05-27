using Atlas.Application.Common;
using Atlas.Application.Users.Register;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class RegisterUserHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AuthSettings _settings = new();

    public RegisterUserHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero));
        _hasher.Hash(Arg.Any<string>()).Returns(PasswordHash.FromHash("stored-hash"));
        _tokens.GenerateUrlSafeToken(Arg.Any<int>()).Returns("raw-token");
        _tokens.Hash(Arg.Any<string>()).Returns("token-hash");
    }

    [Fact]
    public async Task Handle_with_new_email_registers_user_account_and_sends_verification()
    {
        _users.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(false);

        Result result = await CreateHandler()
            .Handle(new RegisterUserCommand("user@example.com", "super-long-password"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _accounts.Received(1).AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _email.Received(1).SendEmailVerificationAsync(
            Arg.Any<EmailAddress>(),
            Arg.Any<UserId>(),
            "raw-token",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_existing_email_fails_without_persisting()
    {
        _users.ExistsByEmailAsync(Arg.Any<EmailAddress>(), Arg.Any<CancellationToken>()).Returns(true);

        Result result = await CreateHandler()
            .Handle(new RegisterUserCommand("user@example.com", "super-long-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.email_already_in_use");
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_invalid_email_fails()
    {
        Result result = await CreateHandler()
            .Handle(new RegisterUserCommand("not-an-email", "super-long-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.invalid_email");
    }

    private RegisterUserHandler CreateHandler() =>
        new(_users, _accounts, _hasher, _tokens, _email, _clock, _unitOfWork, _settings);
}
