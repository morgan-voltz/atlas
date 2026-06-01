using Atlas.Application.Favorites.FavoriteRefresh;
using Atlas.Domain.Common;
using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Favorites;

public class RefreshFavoritesHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 28, 10, 0, 0, TimeSpan.Zero);

    private readonly ICompanyFavoriteRepository _favorites = Substitute.For<ICompanyFavoriteRepository>();
    private readonly ICompanyFavoriteSnapshotRepository _snapshots = Substitute.For<ICompanyFavoriteSnapshotRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IInpiCredentialsRepository _inpiCredentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ICompanyDataProvider _companyProvider = Substitute.For<ICompanyDataProvider>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    public RefreshFavoritesHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _crypto.Decrypt(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) + "-decrypted");
    }

    private RefreshFavoritesHandler CreateHandler() => new(
        _favorites,
        _snapshots,
        _users,
        _inpiCredentials,
        _crypto,
        _companyProvider,
        _clock,
        _unitOfWork,
        _publisher,
        NullLogger<RefreshFavoritesHandler>.Instance);

    private static User NewUser(out EmailAddress email)
    {
        email = EmailAddress.Create($"u-{Guid.NewGuid():N}@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("hash"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }

    private static UniteLegale UniteLegale(string sirenValue, string denomination, string? forme = "SAS", string? naf = "62.01Z") =>
        new(
            Siren.Create(sirenValue).Value,
            denomination,
            forme,
            naf is null ? null : new Naf(naf, "Activité"),
            new Address("1 rue de la Paix", "75001", "Paris", "France"),
            new DateOnly(2010, 1, 1),
            IsDiffusible: true,
            Dirigeants: []);

    [Fact]
    public async Task User_without_inpi_credentials_is_skipped()
    {
        UserId userId = UserId.New();
        _snapshots.GetUserIdsWithFavoritesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserId> { userId });
        _inpiCredentials.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<FavoriteRefreshSummary> result = await CreateHandler()
            .Handle(new RefreshFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UsersProcessed.Should().Be(0);
        await _publisher.DidNotReceive().Publish(Arg.Any<CompanyFavoriteChangedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task First_run_captures_initial_snapshot_without_notification()
    {
        User user = NewUser(out _);
        Siren siren = Siren.Create("552032534").Value;
        SetupConnectedUserWithFavorite(user, siren);
        _snapshots.GetByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavoriteSnapshot>());
        _companyProvider.GetBySirenAsync(siren, Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Ok(UniteLegale("552032534", "Renault")));

        Result<FavoriteRefreshSummary> result = await CreateHandler()
            .Handle(new RefreshFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FavoritesProcessed.Should().Be(1);
        result.Value!.FavoritesWithChanges.Should().Be(0);
        await _snapshots.Received(1).AddAsync(Arg.Any<CompanyFavoriteSnapshot>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<CompanyFavoriteChangedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Detected_change_publishes_notification_and_replaces_snapshot()
    {
        User user = NewUser(out _);
        Siren siren = Siren.Create("552032534").Value;
        SetupConnectedUserWithFavorite(user, siren);

        UniteLegale previous = UniteLegale("552032534", "Renault");
        UniteLegale current = UniteLegale("552032534", "Renault SAS"); // dénomination modifiée

        CompanyFavoriteSnapshot previousSnap = CompanyFavoriteSnapshot.Capture(user.Id, previous, Now.AddDays(-1));
        _snapshots.GetByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavoriteSnapshot> { previousSnap });
        _companyProvider.GetBySirenAsync(siren, Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Ok(current));

        Result<FavoriteRefreshSummary> result = await CreateHandler()
            .Handle(new RefreshFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FavoritesWithChanges.Should().Be(1);
        await _snapshots.Received(1).RemoveAsync(previousSnap, Arg.Any<CancellationToken>());
        await _snapshots.Received(1).AddAsync(Arg.Is<CompanyFavoriteSnapshot>(s => s.Denomination == "Renault SAS"), Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<CompanyFavoriteChangedNotification>(n =>
                n.UserId == user.Id
                && n.SirenValue == "552032534"
                && n.Changes.Any(c => c.Field == "Denomination")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task No_change_does_not_publish_notification()
    {
        User user = NewUser(out _);
        Siren siren = Siren.Create("552032534").Value;
        SetupConnectedUserWithFavorite(user, siren);

        UniteLegale company = UniteLegale("552032534", "Renault");
        CompanyFavoriteSnapshot previousSnap = CompanyFavoriteSnapshot.Capture(user.Id, company, Now.AddDays(-1));
        _snapshots.GetByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavoriteSnapshot> { previousSnap });
        _companyProvider.GetBySirenAsync(siren, Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Ok(company));

        Result<FavoriteRefreshSummary> result = await CreateHandler()
            .Handle(new RefreshFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FavoritesWithChanges.Should().Be(0);
        await _publisher.DidNotReceive().Publish(Arg.Any<CompanyFavoriteChangedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Processes_all_favorites_when_user_has_several()
    {
        // Couvre le fetch parallèle borné (M5) : tous les favoris sont traités, quel que soit l'ordre.
        User user = NewUser(out _);
        Siren siren1 = Siren.Create("552032534").Value;
        Siren siren2 = Siren.Create("652014051").Value;

        _snapshots.GetUserIdsWithFavoritesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserId> { user.Id });
        _inpiCredentials.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(user.Id, "enc-u", "enc-p", Now));
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _favorites.GetByUserAsync(user.Id, Arg.Any<CancellationToken>()).Returns(new List<CompanyFavorite>
        {
            CompanyFavorite.Mark(user.Id, siren1, "Renault", Now),
            CompanyFavorite.Mark(user.Id, siren2, "Carrefour", Now),
        });
        _snapshots.GetByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavoriteSnapshot>()); // premier passage pour les deux
        _companyProvider.GetBySirenAsync(siren1, Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Ok(UniteLegale("552032534", "Renault")));
        _companyProvider.GetBySirenAsync(siren2, Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Ok(UniteLegale("652014051", "Carrefour")));

        Result<FavoriteRefreshSummary> result = await CreateHandler()
            .Handle(new RefreshFavoritesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FavoritesProcessed.Should().Be(2);
        await _snapshots.Received(2).AddAsync(Arg.Any<CompanyFavoriteSnapshot>(), Arg.Any<CancellationToken>());
    }

    private void SetupConnectedUserWithFavorite(User user, Siren siren)
    {
        _snapshots.GetUserIdsWithFavoritesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserId> { user.Id });
        _inpiCredentials.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(user.Id, "enc-u", "enc-p", Now));
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _favorites.GetByUserAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<CompanyFavorite> { CompanyFavorite.Mark(user.Id, siren, "Renault", Now) });
    }
}
