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
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        IFeedSourceRepository repository = services.GetRequiredService<IFeedSourceRepository>();
        IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();
        IDateTimeProvider clock = services.GetRequiredService<IDateTimeProvider>();

        var interval = TimeSpan.FromMinutes(30);
        bool added = false;

        foreach (VeilleCatalog.CatalogSource source in VeilleCatalog.AllSources)
        {
            Result<FeedSource> created = FeedSource.Create(source.Name, source.Url, source.Type, interval, clock.UtcNow);
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
