using Atlas.Domain.Common;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Atlas.Application.Veille.MatchFavoritesInFeedItems;

internal sealed class MatchFavoritesInFeedItemsHandler(
    ICompanyFavoriteRepository favorites,
    IFeedItemFavoriteMatchRepository matches,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    ILogger<MatchFavoritesInFeedItemsHandler> logger)
    : IRequestHandler<MatchFavoritesInFeedItemsCommand, Result<FavoriteMatchSummary>>
{
    /// <summary>Nom minimal pour être considéré comme un signal de matching exploitable.</summary>
    private const int MinNameLength = 3;

    public async Task<Result<FavoriteMatchSummary>> Handle(
        MatchFavoritesInFeedItemsCommand request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CompanyFavorite> all = await favorites.GetAllAsync(cancellationToken);

        // Groupes par user : un user → ses favoris avec un NameSnapshot exploitable.
        var byUser = all
            .Where(f => !string.IsNullOrWhiteSpace(f.NameSnapshot) && f.NameSnapshot!.Trim().Length >= MinNameLength)
            .GroupBy(f => f.UserId)
            .ToList();

        int usersScanned = 0;
        int itemsScanned = 0;
        int matchesCreated = 0;

        foreach (IGrouping<UserId, CompanyFavorite> group in byUser)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserId userId = group.Key;
            var userFavorites = group.ToList();

            IReadOnlyList<FeedItem> candidates =
                await matches.GetCandidatesForUserAsync(userId, request.LookbackDays, cancellationToken);
            if (candidates.Count == 0)
            {
                continue;
            }

            usersScanned++;
            itemsScanned += candidates.Count;

            DateTimeOffset now = clock.UtcNow;
            var newMatches = new List<FeedItemFavoriteMatch>();

            foreach (FeedItem item in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (CompanyFavorite favorite in userFavorites)
                {
                    string name = favorite.NameSnapshot!.Trim();
                    if (MentionsName(item.Title, item.Summary, name))
                    {
                        newMatches.Add(FeedItemFavoriteMatch.Create(
                            item.Id,
                            userId,
                            favorite.Siren,
                            name,
                            now));
                    }
                }
            }

            if (newMatches.Count > 0)
            {
                await matches.AddRangeAsync(newMatches, cancellationToken);
                matchesCreated += newMatches.Count;
            }
        }

        if (matchesCreated > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Favoris/Veille : {Users} user(s), {Items} item(s) scannés, {Matches} mention(s) créée(s).",
                usersScanned,
                itemsScanned,
                matchesCreated);
        }

        return Result<FavoriteMatchSummary>.Ok(new FavoriteMatchSummary(usersScanned, itemsScanned, matchesCreated));
    }

    /// <summary>
    /// Le titre ou le résumé contient-il le nom de l'entreprise comme mot entier (case-insensitive) ?
    /// Algorithme MVP : <c>Contains</c> normalisé avec frontières « non-lettre » de chaque côté pour limiter
    /// les faux positifs (ex. « Total » ne matche pas « TotalEnergies »).
    /// </summary>
    private static bool MentionsName(string title, string? summary, string name)
    {
        string needle = name.Trim();
        if (needle.Length < MinNameLength)
        {
            return false;
        }

        return ContainsWord(title, needle) || (summary is not null && ContainsWord(summary, needle));
    }

    private static bool ContainsWord(string haystack, string needle)
    {
        int index = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        while (index >= 0)
        {
            bool startOk = index == 0 || !char.IsLetterOrDigit(haystack[index - 1]);
            int end = index + needle.Length;
            bool endOk = end >= haystack.Length || !char.IsLetterOrDigit(haystack[end]);

            if (startOk && endOk)
            {
                return true;
            }

            index = haystack.IndexOf(needle, index + 1, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
