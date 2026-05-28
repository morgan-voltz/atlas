using Atlas.Application.Veille.MatchFavoritesInFeedItems;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Veille;

public class MatchFavoritesInFeedItemsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 28, 10, 0, 0, TimeSpan.Zero);

    private readonly ICompanyFavoriteRepository _favorites = Substitute.For<ICompanyFavoriteRepository>();
    private readonly IFeedItemFavoriteMatchRepository _matches = Substitute.For<IFeedItemFavoriteMatchRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public MatchFavoritesInFeedItemsHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    private MatchFavoritesInFeedItemsHandler CreateHandler() => new(
        _favorites,
        _matches,
        _clock,
        _unitOfWork,
        NullLogger<MatchFavoritesInFeedItemsHandler>.Instance);

    private static FeedItem NewItem(string title, string? summary = null) =>
        FeedItem.Create(new FeedSourceId(Guid.NewGuid()), title, "https://x.test/" + Guid.NewGuid(), summary, Now, null, Now);

    private static CompanyFavorite NewFavorite(UserId userId, string siren, string name) =>
        CompanyFavorite.Mark(userId, Siren.Create(siren).Value, name, Now);

    [Fact]
    public async Task Match_detects_company_name_in_title_as_whole_word()
    {
        UserId userId = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite> { NewFavorite(userId, "552032534", "Renault") });

        FeedItem item = NewItem("Renault annonce une nouvelle usine");
        _matches.GetCandidatesForUserAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FeedItem> { item });

        Result<FavoriteMatchSummary> result = await CreateHandler()
            .Handle(new MatchFavoritesInFeedItemsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MatchesCreated.Should().Be(1);
        await _matches.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<FeedItemFavoriteMatch>>(list => list.Any(m => m.MatchedName == "Renault")),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Match_does_not_create_false_positive_on_substring()
    {
        // « Total » ne doit pas matcher « TotalEnergies » (frontières mot).
        UserId userId = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite> { NewFavorite(userId, "542051180", "Total") });

        FeedItem item = NewItem("TotalEnergies inaugure un parc solaire");
        _matches.GetCandidatesForUserAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FeedItem> { item });

        Result<FavoriteMatchSummary> result = await CreateHandler()
            .Handle(new MatchFavoritesInFeedItemsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MatchesCreated.Should().Be(0);
        await _matches.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<FeedItemFavoriteMatch>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Match_detects_name_in_summary_case_insensitive()
    {
        UserId userId = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite> { NewFavorite(userId, "552032534", "Renault") });

        FeedItem item = NewItem("Marché auto", "Le constructeur RENAULT a publié ses résultats trimestriels.");
        _matches.GetCandidatesForUserAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FeedItem> { item });

        Result<FavoriteMatchSummary> result = await CreateHandler()
            .Handle(new MatchFavoritesInFeedItemsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MatchesCreated.Should().Be(1);
    }

    [Fact]
    public async Task Match_ignores_short_names_to_avoid_noise()
    {
        // Noms < 3 caractères ignorés (signal trop court).
        UserId userId = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite> { NewFavorite(userId, "552032534", "GE") });

        FeedItem item = NewItem("GE annonce de bons résultats");
        _matches.GetCandidatesForUserAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FeedItem> { item });

        Result<FavoriteMatchSummary> result = await CreateHandler()
            .Handle(new MatchFavoritesInFeedItemsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MatchesCreated.Should().Be(0);
    }

    [Fact]
    public async Task Match_skips_users_without_favorites_with_a_usable_name()
    {
        UserId userId = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite>
            {
                CompanyFavorite.Mark(userId, Siren.Create("552032534").Value, nameSnapshot: null, Now),
            });

        Result<FavoriteMatchSummary> result = await CreateHandler()
            .Handle(new MatchFavoritesInFeedItemsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UsersScanned.Should().Be(0);
        await _matches.DidNotReceive().GetCandidatesForUserAsync(Arg.Any<UserId>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Match_does_not_call_SaveChanges_when_no_match_created()
    {
        UserId userId = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite> { NewFavorite(userId, "552032534", "Renault") });

        FeedItem item = NewItem("Vente automobile : record en juin");
        _matches.GetCandidatesForUserAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<FeedItem> { item });

        Result<FavoriteMatchSummary> result = await CreateHandler()
            .Handle(new MatchFavoritesInFeedItemsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MatchesCreated.Should().Be(0);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
