using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide la pagination keyset de la timeline contre PostgreSQL réel : parcourir page par page via le
/// curseur couvre EXACTEMENT les mêmes items, dans le même ordre, sans saut ni doublon — y compris à
/// travers une frontière d'ex-aequo d'horodatage (plusieurs items au même <c>PublishedAt</c>).
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class TimelineKeysetTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Paging_by_cursor_covers_all_items_without_skip_or_duplicate_across_ties()
    {
        var userId = UserId.New();
        FeedSource source = FeedSource.Create(
            "Source", $"https://feed.test/{Guid.NewGuid()}", FeedSourceType.Rss, TimeSpan.FromMinutes(30), Now).Value!;

        // 7 items : 4 partagent EXACTEMENT le même horodatage (ex-aequo, cas critique du keyset), 3 distincts.
        var items = new List<FeedItem>();
        for (int i = 0; i < 4; i++)
        {
            items.Add(NewItem(source.Id, $"Ex-aequo {i}", Now));
        }
        items.Add(NewItem(source.Id, "Plus ancien 1", Now.AddHours(-1)));
        items.Add(NewItem(source.Id, "Plus ancien 2", Now.AddHours(-2)));
        items.Add(NewItem(source.Id, "Plus ancien 3", Now.AddHours(-3)));

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(NewUser(userId));
            await context.FeedSources.AddAsync(source);
            await context.FeedItems.AddRangeAsync(items);
            await context.VeilleSubscriptions.AddAsync(VeilleSubscription.Create(userId, source.Id, Now));
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            var repo = new FeedItemRepository(context);

            // Référence : tout en une fois (ordre canonique PublishedAt DESC, Id DESC).
            IReadOnlyList<TimelineEntry> reference =
                await repo.GetTimelineAsync(userId, new TimelineFilter(), cursor: null, limit: 100);
            reference.Should().HaveCount(7);

            // Parcours page par page (limit=2) via le curseur.
            var paged = new List<Guid>();
            TimelineCursor? cursor = null;
            for (int guard = 0; guard < 10; guard++)
            {
                IReadOnlyList<TimelineEntry> pageItems =
                    await repo.GetTimelineAsync(userId, new TimelineFilter(), cursor, limit: 2);
                if (pageItems.Count == 0)
                {
                    break;
                }

                paged.AddRange(pageItems.Select(e => e.Item.Id.Value));
                TimelineEntry last = pageItems[^1];
                cursor = new TimelineCursor(last.Item.PublishedAt, last.Item.Id.Value);
            }

            // Le parcours keyset reproduit exactement l'ordre canonique, sans doublon ni item manquant.
            paged.Should().Equal(reference.Select(e => e.Item.Id.Value));
            paged.Should().OnlyHaveUniqueItems();
        }
    }

    private static FeedItem NewItem(FeedSourceId sourceId, string title, DateTimeOffset publishedAt) =>
        FeedItem.Create(sourceId, title, $"https://feed.test/{Guid.NewGuid()}", "résumé", publishedAt, null, Now);

    private static User NewUser(UserId id) =>
        User.Register(id, EmailAddress.Create($"keyset-{Guid.NewGuid():N}@example.com").Value!,
            PasswordHash.FromHash("stored"), "tok", Now, TimeSpan.FromHours(24));
}
