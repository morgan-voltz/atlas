using System.Globalization;
using System.Net.Http.Json;
using Atlas.Domain.Bodacc;
using Atlas.Domain.Companies;
using Atlas.Infrastructure.Bodacc.Dtos;
using Atlas.Shared.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Bodacc;

/// <summary>
/// Adapter BODACC via l'API Opendatasoft de data.gouv.fr (F-048). API publique anonyme :
/// pas d'auth. Filtre par SIREN via <c>where=search(registre, "...")</c> et tri par date desc.
/// </summary>
internal sealed class OpendatasoftBodaccProvider(
    HttpClient httpClient,
    IOptions<BodaccOptions> options,
    ILogger<OpendatasoftBodaccProvider> logger) : IBodaccProvider
{
    private readonly BodaccOptions _options = options.Value;

    public async Task<Result<IReadOnlyList<BodaccAnnouncement>>> GetAnnouncementsAsync(
        Siren siren,
        DateTimeOffset since,
        int limit,
        CancellationToken ct = default)
    {
        int effectiveLimit = Math.Min(limit, _options.MaxResultsPerCall);

        // L'API Opendatasoft : where=search(registre, "...") + filtrage date côté serveur.
        // On encode soigneusement les paramètres pour éviter les surprises.
        string where = $"search(registre, \"{siren.Value}\") AND dateparution >= date'{since:yyyy-MM-dd}'";
        string url = $"?where={Uri.EscapeDataString(where)}&order_by=dateparution%20desc&limit={effectiveLimit.ToString(CultureInfo.InvariantCulture)}";

        BodaccRecordsResponseDto? body;
        try
        {
            body = await httpClient.GetFromJsonAsync<BodaccRecordsResponseDto>(url, ct);
        }
        catch (HttpRequestException ex)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "BODACC : appel HTTP en échec pour SIREN {Siren}.", siren.Value);
            }
            return Result<IReadOnlyList<BodaccAnnouncement>>.Fail(BodaccErrors.Unavailable);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(ex, "BODACC : timeout pour SIREN {Siren}.", siren.Value);
            }
            return Result<IReadOnlyList<BodaccAnnouncement>>.Fail(BodaccErrors.Unavailable);
        }

        if (body is null)
        {
            return Result<IReadOnlyList<BodaccAnnouncement>>.Fail(BodaccErrors.InvalidResponse("réponse vide"));
        }

        var announcements = new List<BodaccAnnouncement>(body.Results.Count);
        foreach (BodaccRecordDto record in body.Results)
        {
            if (string.IsNullOrWhiteSpace(record.Id) || string.IsNullOrWhiteSpace(record.DateParution))
            {
                continue;
            }

            if (!DateTimeOffset.TryParse(record.DateParution, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset publishedAt))
            {
                continue;
            }

            string typeLabel = record.TypeLabel?.Trim() ?? "Annonce";
            string excerpt = BuildExcerpt(record);

            announcements.Add(new BodaccAnnouncement(
                AnnouncementId: record.Id.Trim(),
                PublishedAt: publishedAt,
                TypeLabel: typeLabel,
                Court: string.IsNullOrWhiteSpace(record.Tribunal) ? null : record.Tribunal.Trim(),
                Excerpt: excerpt));
        }

        return Result<IReadOnlyList<BodaccAnnouncement>>.Ok(announcements);
    }

    private static string BuildExcerpt(BodaccRecordDto record)
    {
        var parts = new List<string>(3);
        if (!string.IsNullOrWhiteSpace(record.Commercant))
        {
            parts.Add(record.Commercant.Trim());
        }
        if (!string.IsNullOrWhiteSpace(record.Ville))
        {
            parts.Add(record.Ville.Trim());
        }
        if (!string.IsNullOrWhiteSpace(record.Tribunal))
        {
            parts.Add($"Tribunal : {record.Tribunal.Trim()}");
        }
        return parts.Count == 0 ? "(détails non disponibles)" : string.Join(" — ", parts);
    }
}
