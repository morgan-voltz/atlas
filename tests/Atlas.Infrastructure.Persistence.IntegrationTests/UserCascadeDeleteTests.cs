using Atlas.Domain.Inpi;
using Atlas.Domain.Search;
using Atlas.Domain.Users;
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
        }
    }
}
