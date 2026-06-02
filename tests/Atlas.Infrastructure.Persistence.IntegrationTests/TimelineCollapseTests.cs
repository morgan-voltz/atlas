using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide le collapse de déduplication de la timeline (F-045) contre un PostgreSQL réel : un cluster
/// multi-sources n'apparaît qu'une fois, via l'item le plus récent PARMI les sources abonnées, et le
/// badge « N sources rapportent » (SourceCount) compte toutes les sources distinctes du cluster.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class TimelineCollapseTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Timeline_collapses_cluster_to_most_recent_subscribed_item()
    {
        var userId = UserId.New();
        FeedSource a = NewSource();
        FeedSource b = NewSource();
        FeedSource c = NewSource();

        // Trois items d'un même cluster ; l'item le plus récent (c) vient d'une source NON abonnée.
        FeedItem itemA = NewItem(a.Id, "Rachat Atlas reporté par A", Now.AddHours(-2));
        FeedItem itemB = NewItem(b.Id, "Rachat Atlas reporté par B", Now);
        FeedItem itemC = NewItem(c.Id, "Rachat Atlas reporté par C", Now.AddHours(1));
        FeedItem standalone = NewItem(a.Id, "Information sans doublon", Now.AddHours(-3));

        FeedItemCluster cluster = FeedItemCluster.Create(123L, itemA, Now);
        cluster.AddItem(itemB);
        cluster.AddItem(itemC);
        itemA.AttachToCluster(cluster.Id);
        itemB.AttachToCluster(cluster.Id);
        itemC.AttachToCluster(cluster.Id);

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(NewUser(userId));
            await context.FeedSources.AddRangeAsync(a, b, c);
            await context.FeedItemClusters.AddAsync(cluster);
            await context.FeedItems.AddRangeAsync(itemA, itemB, itemC, standalone);
            await context.VeilleSubscriptions.AddRangeAsync(
                VeilleSubscription.Create(userId, a.Id, Now),
                VeilleSubscription.Create(userId, b.Id, Now));
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new FeedItemRepository(context);
            IReadOnlyList<TimelineEntry> result =
                await repo.GetTimelineAsync(userId, new TimelineFilter(), cursor: null, limit: 20);

            // Le cluster est réduit à un seul représentant : itemB (le plus récent parmi A et B, les sources
            // abonnées) ; itemC est plus récent mais sa source n'est pas suivie. Le standalone reste.
            result.Should().HaveCount(2);
            result.Select(e => e.Item.Id).Should().Equal(itemB.Id, standalone.Id);

            result.Single(e => e.Item.Id == itemB.Id).SourceCount.Should().Be(3);
            result.Single(e => e.Item.Id == standalone.Id).SourceCount.Should().Be(1);
        }
    }

    private static FeedSource NewSource() =>
        FeedSource.Create("Source test", $"https://feed.test/{Guid.NewGuid()}", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;

    private static FeedItem NewItem(FeedSourceId sourceId, string title, DateTimeOffset publishedAt) =>
        FeedItem.Create(sourceId, title, $"https://feed.test/{Guid.NewGuid()}", "résumé", publishedAt, null, Now);

    // La FK user_id -> users (cascade RGPD, Lot 1) impose une ligne users réelle pour les abonnements.
    private static User NewUser(UserId id) =>
        User.Register(id, EmailAddress.Create($"collapse-{Guid.NewGuid():N}@example.com").Value!,
            PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
}
