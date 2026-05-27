using Atlas.Domain.Search;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Persistence;
using Atlas.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide la rétention de l'historique (prune) contre un PostgreSQL réel.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class SearchHistoryPruneTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Start = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Prune_keeps_only_the_most_recent_entries()
    {
        EmailAddress email = EmailAddress.Create($"prune-{Guid.NewGuid():N}@example.com").Value!;
        var user = User.Register(UserId.New(), email, PasswordHash.FromHash("stored"), "tok", Start, TimeSpan.FromHours(24));
        UserId userId = user.Id;

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(user);
            for (int index = 0; index < 10; index++)
            {
                await context.SearchHistory.AddAsync(
                    SearchHistoryEntry.Record(userId, SearchType.CompanyByName, $"query-{index}", Start.AddSeconds(index)));
            }

            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repository = new SearchHistoryRepository(context);
            await repository.PruneAsync(userId, maxEntries: 3, CancellationToken.None);
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            List<SearchHistoryEntry> remaining = await context.SearchHistory
                .Where(entry => entry.UserId == userId)
                .OrderByDescending(entry => entry.CreatedAt)
                .ToListAsync();

            remaining.Should().HaveCount(3);
            remaining.Select(entry => entry.Query).Should().Equal("query-9", "query-8", "query-7");
        }
    }
}
