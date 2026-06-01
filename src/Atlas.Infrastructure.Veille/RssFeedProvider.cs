using System.Xml;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Parser;
using CodeHollowFeedItem = CodeHollow.FeedReader.FeedItem;

namespace Atlas.Infrastructure.Veille;

/// <summary>
/// Adapter RSS/Atom (<see cref="IExternalContentSource"/>) basé sur CodeHollow.FeedReader.
/// Récupère le flux via un <see cref="HttpClient"/> typé puis le parse depuis les octets (l'encodage
/// déclaré dans le prologue XML est respecté). Ne lève jamais : tout échec → Result.Fail.
/// </summary>
internal sealed class RssFeedProvider(HttpClient httpClient) : IExternalContentSource
{
    private static readonly Result<IReadOnlyList<FeedItemDraft>> Failure =
        Result<IReadOnlyList<FeedItemDraft>>.Fail(VeilleErrors.FetchFailed);

    public bool CanHandle(FeedSourceType type) => type is FeedSourceType.Rss or FeedSourceType.Atom;

    public async Task<Result<IReadOnlyList<FeedItemDraft>>> FetchAsync(
        FeedSource source,
        DateTimeOffset? since,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        byte[] payload;
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(source.Url, ct);
            if (!response.IsSuccessStatusCode)
            {
                return Failure;
            }

            payload = await response.Content.ReadAsByteArrayAsync(ct);
        }
        catch (HttpRequestException)
        {
            return Failure;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return Failure;
        }

        Feed feed;
        try
        {
            // Durcissement anti-XXE (audit Lot 1) : on rejette toute DTD avant de confier le flux
            // à FeedReader, qui parse du XML provenant d'une URL fournie par l'utilisateur.
            SafeXmlGuard.EnsureSafe(payload);
            feed = FeedReader.ReadFromByteArray(payload);
        }
        catch (XmlException)
        {
            return Failure;
        }
        catch (FeedTypeNotSupportedException)
        {
            return Failure;
        }
        catch (ArgumentException)
        {
            return Failure;
        }

        var drafts = new List<FeedItemDraft>(feed.Items.Count);
        foreach (CodeHollowFeedItem item in feed.Items)
        {
            DateTimeOffset publishedAt = ToUtc(item.PublishingDate);
            if (since.HasValue && publishedAt <= since.Value)
            {
                continue;
            }

            IReadOnlyList<string> categories = item.Categories is { Count: > 0 }
                ? item.Categories.ToList()
                : [];

            drafts.Add(new FeedItemDraft(
                item.Title ?? string.Empty,
                item.Link,
                string.IsNullOrWhiteSpace(item.Description) ? item.Content : item.Description,
                publishedAt,
                categories));
        }

        return Result<IReadOnlyList<FeedItemDraft>>.Ok(drafts);
    }

    private static DateTimeOffset ToUtc(DateTime? value)
    {
        DateTime raw = value ?? DateTime.UtcNow;
        DateTime utc = raw.Kind switch
        {
            DateTimeKind.Utc => raw,
            DateTimeKind.Local => raw.ToUniversalTime(),
            _ => DateTime.SpecifyKind(raw, DateTimeKind.Utc),
        };
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }
}
