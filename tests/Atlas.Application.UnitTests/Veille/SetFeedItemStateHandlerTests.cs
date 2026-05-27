using Atlas.Application.Veille.SetFeedItemState;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public sealed class SetFeedItemStateHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IFeedItemRepository _itemRepo = Substitute.For<IFeedItemRepository>();
    private readonly IFeedItemUserStateRepository _stateRepo = Substitute.For<IFeedItemUserStateRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public SetFeedItemStateHandlerTests() => _clock.UtcNow.Returns(Now);

    private SetFeedItemStateHandler CreateHandler() => new(_itemRepo, _stateRepo, _clock, _unitOfWork);

    [Fact]
    public async Task Handle_returns_not_found_when_item_absent()
    {
        _itemRepo.ExistsAsync(Arg.Any<FeedItemId>(), Arg.Any<CancellationToken>()).Returns(false);

        Result<FeedItemStateDto> result = await CreateHandler().Handle(
            new SetFeedItemStateCommand(Guid.NewGuid(), Guid.NewGuid(), true, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("veille.feed_item_not_found");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_creates_state_when_absent()
    {
        _itemRepo.ExistsAsync(Arg.Any<FeedItemId>(), Arg.Any<CancellationToken>()).Returns(true);
        _stateRepo.GetAsync(Arg.Any<UserId>(), Arg.Any<FeedItemId>(), Arg.Any<CancellationToken>())
            .Returns((FeedItemUserState?)null);

        Result<FeedItemStateDto> result = await CreateHandler().Handle(
            new SetFeedItemStateCommand(Guid.NewGuid(), Guid.NewGuid(), true, true, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsRead.Should().BeTrue();
        result.Value!.IsFavorite.Should().BeTrue();
        result.Value!.IsArchived.Should().BeFalse();
        await _stateRepo.Received(1).AddAsync(Arg.Any<FeedItemUserState>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_updates_existing_state_with_partial_flags()
    {
        var userId = UserId.New();
        var itemId = FeedItemId.New();
        FeedItemUserState existing = FeedItemUserState.Create(userId, itemId, Now);
        existing.SetRead(true, Now);
        existing.SetFavorite(true, Now);

        _itemRepo.ExistsAsync(itemId, Arg.Any<CancellationToken>()).Returns(true);
        _stateRepo.GetAsync(userId, itemId, Arg.Any<CancellationToken>()).Returns(existing);

        // On n'archive que : isRead et isFavorite restent inchangés (null).
        Result<FeedItemStateDto> result = await CreateHandler().Handle(
            new SetFeedItemStateCommand(userId.Value, itemId.Value, null, null, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsRead.Should().BeTrue();      // inchangé
        result.Value!.IsFavorite.Should().BeTrue();  // inchangé
        result.Value!.IsArchived.Should().BeTrue();   // appliqué
        _stateRepo.Received(1).Update(existing);
        await _stateRepo.DidNotReceive().AddAsync(Arg.Any<FeedItemUserState>(), Arg.Any<CancellationToken>());
    }
}
