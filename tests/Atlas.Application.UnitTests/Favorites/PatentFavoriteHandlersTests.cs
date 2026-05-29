using Atlas.Application.Favorites;
using Atlas.Application.Favorites.AddPatentFavorite;
using Atlas.Application.Favorites.GetMyPatentFavorites;
using Atlas.Application.Favorites.RemovePatentFavorite;
using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Favorites;

public class PatentFavoriteHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 10, 0, 0, TimeSpan.Zero);
    private const string Number = "FR3045678B1";

    private readonly IPatentFavoriteRepository _favorites = Substitute.For<IPatentFavoriteRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public PatentFavoriteHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    [Fact]
    public async Task Add_returns_invalid_publication_number_on_short_input()
    {
        Result result = await new AddPatentFavoriteHandler(_favorites, _clock, _unitOfWork)
            .Handle(new AddPatentFavoriteCommand(Guid.NewGuid(), "FR", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.invalid_publication_number");
    }

    [Fact]
    public async Task Add_normalizes_input_and_persists()
    {
        Guid userId = Guid.NewGuid();

        Result result = await new AddPatentFavoriteHandler(_favorites, _clock, _unitOfWork)
            .Handle(new AddPatentFavoriteCommand(userId, "fr 30 45 678 b1", "Système de freinage"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _favorites.Received(1).AddAsync(
            Arg.Is<PatentFavorite>(f =>
                f.PublicationNumber.Value == Number && f.TitleSnapshot == "Système de freinage"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_returns_already_favorite_when_existing()
    {
        Guid userId = Guid.NewGuid();
        var number = PublicationNumber.FromTrustedValue(Number);
        _favorites.GetByUserAndNumberAsync(new UserId(userId), number, Arg.Any<CancellationToken>())
            .Returns(PatentFavorite.Mark(new UserId(userId), number, "Freinage", Now));

        Result result = await new AddPatentFavoriteHandler(_favorites, _clock, _unitOfWork)
            .Handle(new AddPatentFavoriteCommand(userId, Number, "Freinage"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.patent_already_favorite");
    }

    [Fact]
    public async Task Remove_returns_not_favorite_when_missing()
    {
        Result result = await new RemovePatentFavoriteHandler(_favorites, _unitOfWork)
            .Handle(new RemovePatentFavoriteCommand(Guid.NewGuid(), Number), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.patent_not_favorite");
    }

    [Fact]
    public async Task GetMine_orders_by_added_at_desc()
    {
        Guid userId = Guid.NewGuid();
        _favorites.GetByUserAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(new List<PatentFavorite>
            {
                PatentFavorite.Mark(new UserId(userId), PublicationNumber.FromTrustedValue("FR1111111"), "A", Now.AddDays(-1)),
                PatentFavorite.Mark(new UserId(userId), PublicationNumber.FromTrustedValue("FR2222222"), "B", Now),
            });

        Result<IReadOnlyList<PatentFavoriteDto>> result =
            await new GetMyPatentFavoritesHandler(_favorites)
                .Handle(new GetMyPatentFavoritesQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value![0].Title.Should().Be("B");
    }
}
