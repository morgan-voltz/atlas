using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Inpi;
using Atlas.Domain.Notifications;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide l'effacement RGPD (F-012) : la suppression d'un utilisateur cascade sur toutes ses données.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class UserCascadeDeleteTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Deleting_user_cascades_to_all_related_data()
    {
        EmailAddress email = EmailAddress.Create($"gdpr-{Guid.NewGuid():N}@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(user);
            await context.Accounts.AddAsync(Account.Create(AccountId.New(), user.Id, Now));
            await context.RefreshTokens.AddAsync(
                RefreshToken.Issue(RefreshTokenId.New(), user.Id, "hash-" + Guid.NewGuid().ToString("N"), Now, TimeSpan.FromDays(30)));
            await context.InpiCredentials.AddAsync(InpiCredentials.Create(user.Id, "enc-u", "enc-p", Now));
            await context.SearchHistory.AddAsync(
                SearchHistoryEntry.Record(user.Id, SearchType.CompanyBySiren, "552032534", Now));

            // Données de veille (F-041 à F-044) : doivent aussi être effacées (RGPD art. 17).
            FeedSource source = FeedSource.Create("Src", $"https://x.test/{Guid.NewGuid():N}", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;
            FeedItem item = FeedItem.Create(source.Id, "Titre", "https://x.test/a", "r", Now, null, Now);
            VeillePack pack = VeillePack.Create($"pack-{Guid.NewGuid():N}", "Pack", "desc", Now).Value!;
            await context.FeedSources.AddAsync(source);
            await context.FeedItems.AddAsync(item);
            await context.VeillePacks.AddAsync(pack);
            await context.VeilleSubscriptions.AddAsync(VeilleSubscription.Create(user.Id, source.Id, Now));
            await context.VeillePackEnrollments.AddAsync(VeillePackEnrollment.Create(user.Id, pack.Id, pack.Version, Now));
            await context.FeedItemUserStates.AddAsync(FeedItemUserState.Create(user.Id, item.Id, Now));

            // Favoris d'entreprises (F-017) : également effacés par cascade RGPD.
            Siren siren = Siren.Create("552032534").Value;
            await context.CompanyFavorites.AddAsync(CompanyFavorite.Mark(user.Id, siren, "Renault", Now));

            // Snapshot favori (F-019) + device push (F-020) : également cascadés.
            UniteLegale snap = new(siren, "Renault", "SAS", null, null, null, true, []);
            await context.CompanyFavoriteSnapshots.AddAsync(CompanyFavoriteSnapshot.Capture(user.Id, snap, Now));
            await context.DeviceRegistrations.AddAsync(
                DeviceRegistration.Register(user.Id, DevicePlatform.FcmAndroid, $"tok-{Guid.NewGuid():N}", "Pixel", Now));

            // Mentions de favoris dans la timeline (F-047) : cascade aussi.
            await context.FeedItemFavoriteMatches.AddAsync(
                FeedItemFavoriteMatch.Create(item.Id, user.Id, siren, "Renault", Now));

            // Événements de favoris (F-047 volet 2) : cascade aussi.
            await context.FavoriteEvents.AddAsync(FavoriteEvent.Record(
                user.Id, siren, FavoriteEventType.RneChanged, "Mise à jour de Renault", "Adresse modifiée.", Now));

            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await new UserRepository(context).DeleteAsync(user.Id, CancellationToken.None);
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            (await context.Users.CountAsync(u => u.Id == user.Id)).Should().Be(0);
            (await context.Accounts.CountAsync(a => a.UserId == user.Id)).Should().Be(0);
            (await context.RefreshTokens.CountAsync(t => t.UserId == user.Id)).Should().Be(0);
            (await context.InpiCredentials.CountAsync(c => c.UserId == user.Id)).Should().Be(0);
            (await context.SearchHistory.CountAsync(e => e.UserId == user.Id)).Should().Be(0);
            (await context.VeilleSubscriptions.CountAsync(s => s.UserId == user.Id)).Should().Be(0);
            (await context.VeillePackEnrollments.CountAsync(en => en.UserId == user.Id)).Should().Be(0);
            (await context.FeedItemUserStates.CountAsync(st => st.UserId == user.Id)).Should().Be(0);
            (await context.CompanyFavorites.CountAsync(f => f.UserId == user.Id)).Should().Be(0);
            (await context.CompanyFavoriteSnapshots.CountAsync(s => s.UserId == user.Id)).Should().Be(0);
            (await context.DeviceRegistrations.CountAsync(d => d.UserId == user.Id)).Should().Be(0);
            (await context.FeedItemFavoriteMatches.CountAsync(m => m.UserId == user.Id)).Should().Be(0);
            (await context.FavoriteEvents.CountAsync(e => e.UserId == user.Id)).Should().Be(0);
        }
    }
}
