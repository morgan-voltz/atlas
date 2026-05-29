using Atlas.Application.Favorites;
using Atlas.Application.Favorites.AddTrademarkFavorite;
using Atlas.Application.Favorites.GetMyTrademarkFavorites;
using Atlas.Application.Favorites.RemoveTrademarkFavorite;
using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Favorites;

public class TrademarkFavoriteHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 10, 0, 0, TimeSpan.Zero);
    private const string DepositValue = "4567890";

    private readonly ITrademarkFavoriteRepository _favorites = Substitute.For<ITrademarkFavoriteRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public TrademarkFavoriteHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Add_returns_invalid_deposit_number_when_blank()
    {
        Result result = await new AddTrademarkFavoriteHandler(_favorites, _clock, _unitOfWork)
            .Handle(new AddTrademarkFavoriteCommand(Guid.NewGuid(), "  ", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.invalid_deposit_number");
    }

    [Fact]
    public async Task Add_returns_already_favorite_when_existing()
    {
        Guid userId = Guid.NewGuid();
        var deposit = new DepositNumber(DepositValue);
        _favorites.GetByUserAndDepositNumberAsync(new UserId(userId), deposit, Arg.Any<CancellationToken>())
            .Returns(TrademarkFavorite.Mark(new UserId(userId), deposit, "Nike", Now));

        Result result = await new AddTrademarkFavoriteHandler(_favorites, _clock, _unitOfWork)
            .Handle(new AddTrademarkFavoriteCommand(userId, DepositValue, "Nike"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.trademark_already_favorite");
        await _favorites.DidNotReceive().AddAsync(Arg.Any<TrademarkFavorite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_persists_and_saves()
    {
        Guid userId = Guid.NewGuid();

        Result result = await new AddTrademarkFavoriteHandler(_favorites, _clock, _unitOfWork)
            .Handle(new AddTrademarkFavoriteCommand(userId, DepositValue, "Nike"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _favorites.Received(1).AddAsync(
            Arg.Is<TrademarkFavorite>(f =>
                f.DepositNumber.Value == DepositValue && f.NameSnapshot == "Nike"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remove_returns_not_favorite_when_missing()
    {
        Result result = await new RemoveTrademarkFavoriteHandler(_favorites, _unitOfWork)
            .Handle(new RemoveTrademarkFavoriteCommand(Guid.NewGuid(), DepositValue), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.trademark_not_favorite");
    }

    [Fact]
    public async Task GetMine_orders_by_added_at_desc()
    {
        Guid userId = Guid.NewGuid();
        _favorites.GetByUserAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(new List<TrademarkFavorite>
            {
                TrademarkFavorite.Mark(new UserId(userId), new DepositNumber("1111111"), "Old", Now.AddDays(-2)),
                TrademarkFavorite.Mark(new UserId(userId), new DepositNumber("2222222"), "New", Now),
            });

        Result<IReadOnlyList<TrademarkFavoriteDto>> result =
            await new GetMyTrademarkFavoritesHandler(_favorites)
                .Handle(new GetMyTrademarkFavoritesQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value![0].Name.Should().Be("New");
        result.Value![1].Name.Should().Be("Old");
    }
}
