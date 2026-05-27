using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Veille;

/// <summary>
/// Amorce quelques sources de veille système au démarrage (idempotent). Dérivé du catalogue doc 07.
/// À terme, ces sources alimenteront les VeillePacks (F-042).
/// </summary>
internal static class FeedSourceSeeder
{
    private static readonly (string Name, string Url, FeedSourceType Type)[] DefaultSources =
    [
        (".NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", FeedSourceType.Rss),
        ("CNIL — Actualités", "https://www.cnil.fr/fr/rss.xml", FeedSourceType.Rss),
        ("data.gouv.fr — Actualités", "https://www.data.gouv.fr/fr/posts/recent.atom", FeedSourceType.Atom),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        IFeedSourceRepository repository = services.GetRequiredService<IFeedSourceRepository>();
        IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();
        IDateTimeProvider clock = services.GetRequiredService<IDateTimeProvider>();

        var interval = TimeSpan.FromMinutes(30);
        bool added = false;

        foreach ((string name, string url, FeedSourceType type) in DefaultSources)
        {
            Result<FeedSource> created = FeedSource.Create(name, url, type, interval, clock.UtcNow);
            if (created.IsFailure)
            {
                continue;
            }

            if (await repository.ExistsByUrlAsync(created.Value!.Url, ct))
            {
                continue;
            }

            await repository.AddAsync(created.Value!, ct);
            added = true;
        }

        if (added)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
