using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Search;

/// <summary>
/// Entrée d'historique de recherche d'un utilisateur (entreprise ou marque). Cf. F-008.
/// </summary>
public sealed class SearchHistoryEntry : Entity<SearchHistoryEntryId>
{
    public const int MaxQueryLength = 256;

    private SearchHistoryEntry()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private SearchHistoryEntry(SearchHistoryEntryId id, UserId userId, SearchType type, string query, DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        Type = type;
        Query = query;
        CreatedAt = createdAt;
    }

    public UserId UserId { get; private set; }

    public SearchType Type { get; private set; }

    public string Query { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public static SearchHistoryEntry Record(UserId userId, SearchType type, string query, DateTimeOffset now)
    {
        string normalized = (query ?? string.Empty).Trim();
        if (normalized.Length > MaxQueryLength)
        {
            normalized = normalized[..MaxQueryLength];
        }

        return new SearchHistoryEntry(SearchHistoryEntryId.New(), userId, type, normalized, now);
    }
}
