using Atlas.Application.Favorites.PollBodacc;
using Atlas.Domain.Bodacc;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Favorites;

public class PollBodaccForFavoritesHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);
    private static readonly Siren Renault = Siren.Create("552032534").Value;

    private readonly ICompanyFavoriteRepository _favorites = Substitute.For<ICompanyFavoriteRepository>();
    private readonly IFavoriteEventRepository _events = Substitute.For<IFavoriteEventRepository>();
    private readonly IBodaccProvider _bodacc = Substitute.For<IBodaccProvider>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public PollBodaccForFavoritesHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    private PollBodaccForFavoritesHandler CreateHandler() => new(
        _favorites, _events, _bodacc, _clock, _unitOfWork,
        NullLogger<PollBodaccForFavoritesHandler>.Instance);

    private static CompanyFavorite NewFavorite(UserId userId, Siren siren, string name) =>
        CompanyFavorite.Mark(userId, siren, name, Now);

    private static BodaccAnnouncement NewAnnouncement(string id, string type = "Création") =>
        new(id, Now.AddDays(-1), type, Court: "Tribunal de commerce de Paris", Excerpt: "Détails de l'annonce.");

    [Fact]
    public async Task Handle_creates_event_per_user_for_each_new_announcement()
    {
        UserId u1 = UserId.New();
        UserId u2 = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CompanyFavorite>
        {
            NewFavorite(u1, Renault, "Renault"),
            NewFavorite(u2, Renault, "Renault"),
        });
        _bodacc.GetAnnouncementsAsync(Renault, Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BodaccAnnouncement>>.Ok(new List<BodaccAnnouncement>
            {
                NewAnnouncement("A-1"),
                NewAnnouncement("A-2", "Procédure collective"),
            }));
        _events.GetKnownExternalIdsAsync(Arg.Any<UserId>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        Result<BodaccPollSummary> result = await CreateHandler()
            .Handle(new PollBodaccForFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SirensQueried.Should().Be(1); // dédup cross-user : 1 appel API pour 2 users.
        result.Value!.EventsCreated.Should().Be(4); // 2 users × 2 annonces.
        await _events.Received(4).AddAsync(Arg.Any<FavoriteEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_skips_already_known_announcements()
    {
        UserId u1 = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CompanyFavorite>
        {
            NewFavorite(u1, Renault, "Renault"),
        });
        _bodacc.GetAnnouncementsAsync(Renault, Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BodaccAnnouncement>>.Ok(new List<BodaccAnnouncement>
            {
                NewAnnouncement("A-1"),
                NewAnnouncement("A-2"),
            }));
        // A-1 déjà connu : on doit créer seulement A-2.
        _events.GetKnownExternalIdsAsync(u1, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<string> { "A-1" });

        Result<BodaccPollSummary> result = await CreateHandler()
            .Handle(new PollBodaccForFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EventsCreated.Should().Be(1);
        await _events.Received(1).AddAsync(
            Arg.Is<FavoriteEvent>(e => e.ExternalId == "A-2" && e.Type == FavoriteEventType.BodaccPublished),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_counts_failure_and_continues_with_other_sirens()
    {
        Siren other = Siren.Create("652014051").Value;
        UserId u1 = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CompanyFavorite>
        {
            NewFavorite(u1, Renault, "Renault"),
            NewFavorite(u1, other, "Carrefour"),
        });
        _bodacc.GetAnnouncementsAsync(Renault, Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BodaccAnnouncement>>.Fail(BodaccErrors.Unavailable));
        _bodacc.GetAnnouncementsAsync(other, Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BodaccAnnouncement>>.Ok(new List<BodaccAnnouncement>
            {
                NewAnnouncement("B-1"),
            }));
        _events.GetKnownExternalIdsAsync(Arg.Any<UserId>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        Result<BodaccPollSummary> result = await CreateHandler()
            .Handle(new PollBodaccForFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SirensQueried.Should().Be(1);
        result.Value!.SirensFailed.Should().Be(1);
        result.Value!.EventsCreated.Should().Be(1);
    }

    [Fact]
    public async Task Handle_does_not_save_when_no_events_created()
    {
        UserId u1 = UserId.New();
        _favorites.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CompanyFavorite>
        {
            NewFavorite(u1, Renault, "Renault"),
        });
        _bodacc.GetAnnouncementsAsync(Renault, Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BodaccAnnouncement>>.Ok(new List<BodaccAnnouncement>()));

        Result<BodaccPollSummary> result = await CreateHandler()
            .Handle(new PollBodaccForFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EventsCreated.Should().Be(0);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Created_event_uses_announcement_published_date_as_occurredAt()
    {
        UserId u1 = UserId.New();
        DateTimeOffset annDate = Now.AddDays(-5);
        _favorites.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<CompanyFavorite>
        {
            NewFavorite(u1, Renault, "Renault"),
        });
        _bodacc.GetAnnouncementsAsync(Renault, Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<BodaccAnnouncement>>.Ok(new List<BodaccAnnouncement>
            {
                new("A-1", annDate, "Création", null, "Excerpt"),
            }));
        _events.GetKnownExternalIdsAsync(Arg.Any<UserId>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        await CreateHandler().Handle(new PollBodaccForFavoritesCommand(), CancellationToken.None);

        // L'OccurredAt doit être la date de l'annonce, pas Now (tri timeline correct).
        await _events.Received(1).AddAsync(
            Arg.Is<FavoriteEvent>(e => e.OccurredAt == annDate),
            Arg.Any<CancellationToken>());
    }
}
