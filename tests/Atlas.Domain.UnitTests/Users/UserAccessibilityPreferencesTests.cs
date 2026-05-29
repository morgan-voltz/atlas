using Atlas.Domain.Users;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Users;

public class UserAccessibilityPreferencesTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void New_user_has_default_accessibility_preferences()
    {
        var user = User.Register(
            UserId.New(),
            EmailAddress.Create("u@example.com").Value!,
            PasswordHash.FromHash("argon2-hash"),
            "tokenhash",
            Now,
            TimeSpan.FromHours(24));

        user.AccessibilityPreferences.Should().Be(UserAccessibilityPreferences.Default);
        user.AccessibilityPreferences.HighContrast.Should().BeFalse();
        user.AccessibilityPreferences.ReduceMotion.Should().BeFalse();
        user.AccessibilityPreferences.FontPreference.Should().Be(AccessibilityFontPreference.Default);
    }

    [Fact]
    public void UpdateAccessibilityPreferences_applies_provided_values()
    {
        var user = User.Register(
            UserId.New(),
            EmailAddress.Create("u@example.com").Value!,
            PasswordHash.FromHash("argon2-hash"),
            "tokenhash",
            Now,
            TimeSpan.FromHours(24));

        var newPrefs = new UserAccessibilityPreferences(
            HighContrast: true,
            ReduceMotion: true,
            FontPreference: AccessibilityFontPreference.DyslexiaFriendly);

        user.UpdateAccessibilityPreferences(newPrefs);

        user.AccessibilityPreferences.Should().Be(newPrefs);
    }

    [Fact]
    public void UpdateAccessibilityPreferences_rejects_null()
    {
        var user = User.Register(
            UserId.New(),
            EmailAddress.Create("u@example.com").Value!,
            PasswordHash.FromHash("argon2-hash"),
            "tokenhash",
            Now,
            TimeSpan.FromHours(24));

        FluentActions.Invoking(() => user.UpdateAccessibilityPreferences(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Default_factory_is_idempotent_and_neutral()
    {
        UserAccessibilityPreferences first = UserAccessibilityPreferences.Default;
        UserAccessibilityPreferences second = UserAccessibilityPreferences.Default;

        first.Should().BeSameAs(second);
        first.HighContrast.Should().BeFalse();
        first.ReduceMotion.Should().BeFalse();
        first.FontPreference.Should().Be(AccessibilityFontPreference.Default);
    }
}
