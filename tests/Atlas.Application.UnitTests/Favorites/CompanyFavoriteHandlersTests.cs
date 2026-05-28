using Atlas.Application.Favorites;
using Atlas.Application.Favorites.AddCompanyFavorite;
using Atlas.Application.Favorites.GetMyCompanyFavorites;
using Atlas.Application.Favorites.RemoveCompanyFavorite;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Favorites;

public class CompanyFavoriteHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 28, 10, 0, 0, TimeSpan.Zero);

    // SIREN valide (passe le Luhn) : Carrefour.
    private const string ValidSiren = "652014051";

    private readonly ICompanyFavoriteRepository _favorites = Substitute.For<ICompanyFavoriteRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public CompanyFavoriteHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    // ── Add ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Add_returns_invalid_siren_when_format_is_wrong()
    {
        var handler = new AddCompanyFavoriteHandler(_favorites, _clock, _unitOfWork);

        Result result = await handler.Handle(
            new AddCompanyFavoriteCommand(Guid.NewGuid(), "ABC", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.invalid_siren");
        await _favorites.DidNotReceive().AddAsync(Arg.Any<CompanyFavorite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_returns_already_favorite_when_existing()
    {
        Guid userId = Guid.NewGuid();
        Siren siren = Siren.Create(ValidSiren).Value;
        _favorites.GetByUserAndSirenAsync(new UserId(userId), siren, Arg.Any<CancellationToken>())
            .Returns(CompanyFavorite.Mark(new UserId(userId), siren, "Carrefour", Now));

        var handler = new AddCompanyFavoriteHandler(_favorites, _clock, _unitOfWork);

        Result result = await handler.Handle(
            new AddCompanyFavoriteCommand(userId, ValidSiren, "Carrefour"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.company_already_favorite");
        await _favorites.DidNotReceive().AddAsync(Arg.Any<CompanyFavorite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_persists_favorite_and_saves_changes()
    {
        Guid userId = Guid.NewGuid();
        var handler = new AddCompanyFavoriteHandler(_favorites, _clock, _unitOfWork);

        Result result = await handler.Handle(
            new AddCompanyFavoriteCommand(userId, ValidSiren, "Carrefour SA"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _favorites.Received(1).AddAsync(
            Arg.Is<CompanyFavorite>(f =>
                f.UserId == new UserId(userId)
                && f.Siren.Value == ValidSiren
                && f.NameSnapshot == "Carrefour SA"
                && f.AddedAt == Now),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── Remove ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remove_returns_not_favorite_when_missing()
    {
        var handler = new RemoveCompanyFavoriteHandler(_favorites, _unitOfWork);

        Result result = await handler.Handle(
            new RemoveCompanyFavoriteCommand(Guid.NewGuid(), ValidSiren),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("favorites.company_not_favorite");
    }

    [Fact]
    public async Task Remove_deletes_existing_favorite_and_saves_changes()
    {
        Guid userId = Guid.NewGuid();
        Siren siren = Siren.Create(ValidSiren).Value;
        CompanyFavorite existing = CompanyFavorite.Mark(new UserId(userId), siren, null, Now);
        _favorites.GetByUserAndSirenAsync(new UserId(userId), siren, Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new RemoveCompanyFavoriteHandler(_favorites, _unitOfWork);

        Result result = await handler.Handle(
            new RemoveCompanyFavoriteCommand(userId, ValidSiren),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _favorites.Received(1).RemoveAsync(existing, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── Get ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_returns_favorites_ordered_by_AddedAt_desc()
    {
        Guid userId = Guid.NewGuid();
        Siren siren1 = Siren.Create(ValidSiren).Value;
        Siren siren2 = Siren.Create("652014051").Value;

        _favorites.GetByUserAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite>
            {
                CompanyFavorite.Mark(new UserId(userId), siren1, "Plus ancien", Now.AddDays(-2)),
                CompanyFavorite.Mark(new UserId(userId), siren2, "Plus récent", Now),
            });

        var handler = new GetMyCompanyFavoritesHandler(_favorites);

        Result<IReadOnlyList<CompanyFavoriteDto>> result =
            await handler.Handle(new GetMyCompanyFavoritesQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value![0].Name.Should().Be("Plus récent");
        result.Value![1].Name.Should().Be("Plus ancien");
    }
}
