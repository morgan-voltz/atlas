using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Persistence.Repositories;
using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide la requête timeline (F-044) contre un PostgreSQL réel : scope aux sources abonnées, exclusion
/// des archivés, filtres unread/favorites/mot-clé, tri chronologique inversé.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class TimelineQueryTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Timeline_scopes_to_subscribed_sources_and_excludes_archived_by_default()
    {
        var userId = UserId.New();
        FeedSource subscribed = NewSource("https://feed.test/sub-" + Guid.NewGuid());
        FeedSource other = NewSource("https://feed.test/other-" + Guid.NewGuid());

        FeedItem a1 = NewItem(subscribed.Id, "Article récent", Now);
        FeedItem a2 = NewItem(subscribed.Id, "Article archivé", Now.AddHours(-1));
        FeedItem a3 = NewItem(subscribed.Id, "Article ancien", Now.AddHours(-2));
        FeedItem b1 = NewItem(other.Id, "Item d'une source non abonnée", Now);

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.FeedSources.AddRangeAsync(subscribed, other);
            await context.FeedItems.AddRangeAsync(a1, a2, a3, b1);
            await context.VeilleSubscriptions.AddAsync(VeilleSubscription.Create(userId, subscribed.Id, Now));
            FeedItemUserState archived = FeedItemUserState.Create(userId, a2.Id, Now);
            archived.SetArchived(true, Now);
            await context.FeedItemUserStates.AddAsync(archived);
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new FeedItemRepository(context);
            PagedResult<TimelineEntry> result =
                await repo.GetTimelineAsync(userId, new TimelineFilter(), page: 1, pageSize: 20);

            // a2 archivé exclu, b1 (source non abonnée) exclu ; tri par PublishedAt desc.
            result.TotalCount.Should().Be(2);
            result.Items.Select(e => e.Item.Id).Should().Equal(a1.Id, a3.Id);
        }
    }

    [Fact]
    public async Task Timeline_applies_favorites_unread_and_keyword_filters()
    {
        var userId = UserId.New();
        FeedSource source = NewSource("https://feed.test/filters-" + Guid.NewGuid());

        FeedItem read = NewItem(source.Id, "Décision foobar du Conseil", Now);
        FeedItem unread = NewItem(source.Id, "Autre article", Now.AddHours(-1));

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.FeedSources.AddAsync(source);
            await context.FeedItems.AddRangeAsync(read, unread);
            await context.VeilleSubscriptions.AddAsync(VeilleSubscription.Create(userId, source.Id, Now));
            FeedItemUserState state = FeedItemUserState.Create(userId, read.Id, Now);
            state.SetRead(true, Now);
            state.SetFavorite(true, Now);
            await context.FeedItemUserStates.AddAsync(state);
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new FeedItemRepository(context);

            PagedResult<TimelineEntry> favorites =
                await repo.GetTimelineAsync(userId, new TimelineFilter(FavoritesOnly: true), 1, 20);
            favorites.Items.Should().ContainSingle().Which.Item.Id.Should().Be(read.Id);
            favorites.Items[0].IsFavorite.Should().BeTrue();
            favorites.Items[0].IsRead.Should().BeTrue();

            PagedResult<TimelineEntry> unreadOnly =
                await repo.GetTimelineAsync(userId, new TimelineFilter(UnreadOnly: true), 1, 20);
            unreadOnly.Items.Should().ContainSingle().Which.Item.Id.Should().Be(unread.Id);

            PagedResult<TimelineEntry> keyword =
                await repo.GetTimelineAsync(userId, new TimelineFilter(Keyword: "foobar"), 1, 20);
            keyword.Items.Should().ContainSingle().Which.Item.Id.Should().Be(read.Id);
        }
    }

    private static FeedSource NewSource(string url) =>
        FeedSource.Create("Source test", url, FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;

    private static FeedItem NewItem(FeedSourceId sourceId, string title, DateTimeOffset publishedAt) =>
        FeedItem.Create(sourceId, title, $"https://feed.test/{Guid.NewGuid()}", "résumé", publishedAt, null, Now);
}
