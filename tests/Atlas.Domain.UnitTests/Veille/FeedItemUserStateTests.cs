using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using FluentAssertions;

namespace Atlas.Domain.UnitTests.Veille;

public sealed class FeedItemUserStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_defaults_to_unread_unfavorited_unarchived()
    {
        var userId = UserId.New();
        var itemId = FeedItemId.New();

        FeedItemUserState state = FeedItemUserState.Create(userId, itemId, Now);

        state.UserId.Should().Be(userId);
        state.FeedItemId.Should().Be(itemId);
        state.IsRead.Should().BeFalse();
        state.IsFavorite.Should().BeFalse();
        state.IsArchived.Should().BeFalse();
        state.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void Setters_update_flag_and_timestamp()
    {
        FeedItemUserState state = FeedItemUserState.Create(UserId.New(), FeedItemId.New(), Now);
        DateTimeOffset later = Now.AddHours(1);

        state.SetRead(true, later);
        state.IsRead.Should().BeTrue();
        state.UpdatedAt.Should().Be(later);

        DateTimeOffset later2 = Now.AddHours(2);
        state.SetFavorite(true, later2);
        state.IsFavorite.Should().BeTrue();
        state.UpdatedAt.Should().Be(later2);

        DateTimeOffset later3 = Now.AddHours(3);
        state.SetArchived(true, later3);
        state.IsArchived.Should().BeTrue();
        state.UpdatedAt.Should().Be(later3);

        // Un drapeau peut être remis à false.
        state.SetRead(false, Now.AddHours(4));
        state.IsRead.Should().BeFalse();
    }
}
