using Atlas.Domain.Common;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.PollFeedSources;

internal sealed class PollFeedSourcesHandler(
    IFeedSourceRepository sourceRepository,
    IFeedItemRepository itemRepository,
    IEnumerable<IExternalContentSource> contentSources,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<PollFeedSourcesCommand, Result<FeedPollSummary>>
{
    public async Task<Result<FeedPollSummary>> Handle(PollFeedSourcesCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.UtcNow;
        IReadOnlyList<FeedSource> sources = await sourceRepository.GetActiveAsync(cancellationToken);

        int polled = 0;
        int added = 0;
        int failed = 0;

        foreach (FeedSource source in sources)
        {
            if (!source.IsDueForPolling(now))
            {
                continue;
            }

            IExternalContentSource? provider = contentSources.FirstOrDefault(candidate => candidate.CanHandle(source.Type));
            if (provider is null)
            {
                failed++;
                continue;
            }

            // Le provider ne lève jamais : il renvoie un Result (flux mort/parse KO → échec géré).
            Result<IReadOnlyList<FeedItemDraft>> fetch = await provider.FetchAsync(source, source.LastPolledAt, cancellationToken);

            // On marque la source comme pollée dans tous les cas pour ne pas marteler une source morte.
            source.MarkPolled(now);
            sourceRepository.Update(source);

            if (fetch.IsFailure)
            {
                failed++;
                continue;
            }

            var items = fetch.Value!
                .Select(draft => FeedItem.Create(
                    source.Id, draft.Title, draft.Url, draft.Summary, draft.PublishedAt, draft.Categories, now))
                .GroupBy(item => item.ContentHash, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();

            IReadOnlyCollection<string> existing = await itemRepository.GetExistingHashesAsync(
                source.Id,
                items.Select(item => item.ContentHash).ToList(),
                cancellationToken);
            var existingSet = existing.ToHashSet(StringComparer.Ordinal);

            var newItems = items.Where(item => !existingSet.Contains(item.ContentHash)).ToList();
            if (newItems.Count > 0)
            {
                await itemRepository.AddRangeAsync(newItems, cancellationToken);
            }

            added += newItems.Count;
            polled++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<FeedPollSummary>.Ok(new FeedPollSummary(polled, added, failed));
    }
}
