using Atlas.Application.Users.Accessibility;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Users;

public class AccessibilityPreferencesHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Get_returns_user_accessibility_preferences()
    {
        User user = ActiveUser();
        user.UpdateAccessibilityPreferences(
            new UserAccessibilityPreferences(true, true, AccessibilityFontPreference.DyslexiaFriendly));
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        Result<AccessibilityPreferencesDto> result = await new GetAccessibilityPreferencesHandler(_users)
            .Handle(new GetAccessibilityPreferencesQuery(user.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.HighContrast.Should().BeTrue();
        result.Value!.ReduceMotion.Should().BeTrue();
        result.Value!.FontPreference.Should().Be(AccessibilityFontPreference.DyslexiaFriendly);
    }

    [Fact]
    public async Task Get_when_user_missing_returns_not_found()
    {
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        Result<AccessibilityPreferencesDto> result = await new GetAccessibilityPreferencesHandler(_users)
            .Handle(new GetAccessibilityPreferencesQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
    }

    [Fact]
    public async Task Update_applies_preferences_and_commits()
    {
        User user = ActiveUser();
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(user);

        Result result = await new UpdateAccessibilityPreferencesHandler(_users, _uow)
            .Handle(
                new UpdateAccessibilityPreferencesCommand(
                    user.Id.Value,
                    HighContrast: true,
                    ReduceMotion: false,
                    FontPreference: AccessibilityFontPreference.HighReadability),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.AccessibilityPreferences.HighContrast.Should().BeTrue();
        user.AccessibilityPreferences.ReduceMotion.Should().BeFalse();
        user.AccessibilityPreferences.FontPreference.Should().Be(AccessibilityFontPreference.HighReadability);
        _users.Received(1).Update(user);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_when_user_missing_returns_not_found()
    {
        _users.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        Result result = await new UpdateAccessibilityPreferencesHandler(_users, _uow)
            .Handle(
                new UpdateAccessibilityPreferencesCommand(
                    Guid.NewGuid(),
                    HighContrast: true,
                    ReduceMotion: true,
                    FontPreference: AccessibilityFontPreference.Default),
                CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("users.not_found");
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static User ActiveUser()
    {
        EmailAddress email = EmailAddress.Create("user@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("hash"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }
}
