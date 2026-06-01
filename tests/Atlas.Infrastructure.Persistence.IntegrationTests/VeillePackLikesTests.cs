using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide contre PostgreSQL réel l'incrément/décrément atomique du compteur de likes (audit Lot 3, M7) :
/// UPDATE ... likes_count = likes_count + 1 et le plancher à 0 (Math.Max → GREATEST) via ExecuteUpdate.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class VeillePackLikesTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Increment_then_decrement_is_atomic_and_floors_at_zero()
    {
        User author = NewUser();
        VeillePack pack = VeillePack.CreateUserPack(
            author.Id, "p" + Guid.NewGuid().ToString("N")[..8], "Pack", "desc", Now).Value!;

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(author);
            await context.VeillePacks.AddAsync(pack);
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new VeillePackRepository(context);
            await repo.IncrementLikesAsync(pack.Id);
            await repo.IncrementLikesAsync(pack.Id);
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            VeillePack loaded = await context.VeillePacks.SingleAsync(p => p.Id == pack.Id);
            loaded.LikesCount.Should().Be(2);
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new VeillePackRepository(context);
            await repo.DecrementLikesAsync(pack.Id);
            await repo.DecrementLikesAsync(pack.Id);
            await repo.DecrementLikesAsync(pack.Id); // 3e décrément : ne doit pas passer sous 0
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            VeillePack loaded = await context.VeillePacks.SingleAsync(p => p.Id == pack.Id);
            loaded.LikesCount.Should().Be(0, "le décrément est borné à 0");
        }
    }

    private static User NewUser()
    {
        EmailAddress email = EmailAddress.Create($"author-{Guid.NewGuid():N}@example.com").Value!;
        User user = User.Register(UserId.New(), email, PasswordHash.FromHash("argon2-hash"), "tok", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tok", Now);
        return user;
    }
}
