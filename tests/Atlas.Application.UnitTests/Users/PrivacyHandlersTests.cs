using Atlas.Application.Users.Privacy;
using Atlas.Domain.Inpi;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class PrivacyHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IInpiCredentialsRepository _inpi = Substitute.For<IInpiCredentialsRepository>();
    private readonly ISearchHistoryRepository _history = Substitute.For<ISearchHistoryRepository>();

    [Fact]
    public async Task DeleteAccount_deletes_the_user()
    {
        var userId = Guid.NewGuid();

        Result result = await new DeleteAccountHandler(_users)
            .Handle(new DeleteAccountCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _users.Received(1).DeleteAsync(
            Arg.Is<UserId>(id => id.Value == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportUserData_aggregates_account_inpi_and_history_without_secrets()
    {
        User user = ActiveUser();
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);
        _inpi.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(user.Id, "enc-u", "enc-p", Now));
        _history.GetRecentByUserAsync(Arg.Any<UserId>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchHistoryEntry>
            {
                SearchHistoryEntry.Record(user.Id, SearchType.CompanyBySiren, "552032534", Now),
            });

        Result<UserDataExportDto> result = await new ExportUserDataHandler(_users, _inpi, _history)
            .Handle(new ExportUserDataQuery(user.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        UserDataExportDto export = result.Value!;
        export.Email.Should().Be("user@example.com");
        export.Status.Should().Be("Active");
        export.TwoFactorEnabled.Should().BeFalse();
        export.InpiConnection!.Status.Should().Be("Active");
        export.SearchHistory.Should().ContainSingle(entry => entry.Query == "552032534");
    }

    [Fact]
    public async Task ExportUserData_when_user_missing_fails()
    {
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        Result<UserDataExportDto> result = await new ExportUserDataHandler(_users, _inpi, _history)
            .Handle(new ExportUserDataQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    private static User ActiveUser()
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }
}
