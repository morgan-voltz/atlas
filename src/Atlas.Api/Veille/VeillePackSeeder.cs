using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Api.Veille;

/// <summary>
/// Amorce les VeillePacks du <see cref="VeilleCatalog"/> (F-042, idempotent). À exécuter après
/// <see cref="FeedSourceSeeder"/> : les packs référencent les <see cref="FeedSource"/> par URL.
/// Upsert par code : création en version 1, ou mise à jour des sources + montée de version si le
/// catalogue déclare une version supérieure (déclenche le signal de re-synchronisation côté abonnés).
/// </summary>
internal static class VeillePackSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        IVeillePackRepository packRepository = services.GetRequiredService<IVeillePackRepository>();
        IFeedSourceRepository sourceRepository = services.GetRequiredService<IFeedSourceRepository>();
        IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();
        IDateTimeProvider clock = services.GetRequiredService<IDateTimeProvider>();

        bool changed = false;

        foreach (VeilleCatalog.CatalogPack definition in VeilleCatalog.Packs)
        {
            IReadOnlyList<FeedSourceId> sourceIds = await ResolveSourceIdsAsync(sourceRepository, definition.SourceUrls, ct);
            if (sourceIds.Count == 0)
            {
                continue;
            }

            VeillePack? existing = await packRepository.GetByCodeAsync(definition.Code, ct);
            if (existing is null)
            {
                Result<VeillePack> created = VeillePack.Create(definition.Code, definition.Name, definition.Description, clock.UtcNow);
                if (created.IsFailure)
                {
                    continue;
                }

                VeillePack pack = created.Value!;
                pack.SetSources(sourceIds);
                await packRepository.AddAsync(pack, ct);
                changed = true;
            }
            else if (definition.Version > existing.Version)
            {
                while (existing.Version < definition.Version)
                {
                    existing.BumpVersion();
                }

                existing.SetSources(sourceIds);
                packRepository.Update(existing);
                changed = true;
            }
        }

        if (changed)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private static async Task<IReadOnlyList<FeedSourceId>> ResolveSourceIdsAsync(
        IFeedSourceRepository sourceRepository,
        IEnumerable<string> urls,
        CancellationToken ct)
    {
        var ids = new List<FeedSourceId>();
        foreach (string url in urls)
        {
            string canonical = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri.ToString() : url;
            FeedSource? source = await sourceRepository.GetByUrlAsync(canonical, ct);
            if (source is not null)
            {
                ids.Add(source.Id);
            }
        }

        return ids;
    }
}
