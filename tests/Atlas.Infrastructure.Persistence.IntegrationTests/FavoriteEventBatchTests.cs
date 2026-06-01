using Atlas.Domain.Companies;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide contre PostgreSQL réel la requête batch des identifiants BODACC connus (audit Lot 3b, E4c) :
/// le filtre <c>userIds.Contains(...)</c> sur des UserId convertis doit se traduire en SQL et regrouper
/// correctement par utilisateur.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class FavoriteEventBatchTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);
    private static readonly Siren Renault = Siren.Create("552032534").Value;

    [Fact]
    public async Task GetKnownExternalIdsForUsers_groups_known_ids_per_user()
    {
        User u1 = NewUser();
        User u2 = NewUser();
        User u3 = NewUser(); // sans aucun événement

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddRangeAsync(u1, u2, u3);
            await context.FavoriteEvents.AddRangeAsync(
                Event(u1.Id, "A-1"),
                Event(u1.Id, "A-2"),
                Event(u2.Id, "A-1"));
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new FavoriteEventRepository(context);

            IReadOnlyDictionary<UserId, IReadOnlyCollection<string>> known =
                await repo.GetKnownExternalIdsForUsersAsync(
                    new[] { u1.Id, u2.Id, u3.Id },
                    new[] { "A-1", "A-2", "A-3" },
                    CancellationToken.None);

            known[u1.Id].Should().BeEquivalentTo("A-1", "A-2");
            known[u2.Id].Should().BeEquivalentTo("A-1");
            known.ContainsKey(u3.Id).Should().BeFalse("aucun événement connu pour u3");
        }
    }

    private static FavoriteEvent Event(UserId userId, string externalId) =>
        FavoriteEvent.Record(userId, Renault, FavoriteEventType.BodaccPublished, "Titre", "Résumé", Now, externalId);

    private static User NewUser()
    {
        EmailAddress email = EmailAddress.Create($"u-{Guid.NewGuid():N}@example.com").Value!;
        User user = User.Register(UserId.New(), email, PasswordHash.FromHash("argon2-hash"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }
}
