using Atlas.Domain.Common;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

/// <summary>
/// Source de contenu daté suivie par le système (RSS/Atom, etc.). Cf. doc 08 §9.2, F-041.
/// </summary>
public sealed class FeedSource : Entity<FeedSourceId>
{
    public const int MaxNameLength = 200;
    public const int MaxUrlLength = 2048;

    private FeedSource()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private FeedSource(
        FeedSourceId id,
        string name,
        string url,
        FeedSourceType type,
        TimeSpan pollingInterval,
        DateTimeOffset createdAt)
        : base(id)
    {
        Name = name;
        Url = url;
        Type = type;
        PollingInterval = pollingInterval;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public string Name { get; private set; } = null!;

    public string Url { get; private set; } = null!;

    public FeedSourceType Type { get; private set; }

    public TimeSpan PollingInterval { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastPolledAt { get; private set; }

    public static Result<FeedSource> Create(
        string name,
        string url,
        FeedSourceType type,
        TimeSpan pollingInterval,
        DateTimeOffset now)
    {
        string trimmedName = (name ?? string.Empty).Trim();
        if (trimmedName.Length is 0 or > MaxNameLength)
        {
            return Result<FeedSource>.Fail(VeilleErrors.InvalidFeedSource("nom requis (≤ 200 caractères)."));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || url.Length > MaxUrlLength)
        {
            return Result<FeedSource>.Fail(VeilleErrors.InvalidFeedSource("URL http(s) absolue requise."));
        }

        if (pollingInterval <= TimeSpan.Zero)
        {
            return Result<FeedSource>.Fail(VeilleErrors.InvalidFeedSource("intervalle de polling positif requis."));
        }

        return Result<FeedSource>.Ok(new FeedSource(FeedSourceId.New(), trimmedName, uri.ToString(), type, pollingInterval, now));
    }

    /// <summary>Vrai si la source est active et n'a pas été pollée depuis au moins son intervalle.</summary>
    public bool IsDueForPolling(DateTimeOffset now) =>
        IsActive && (LastPolledAt is null || now - LastPolledAt.Value >= PollingInterval);

    public void MarkPolled(DateTimeOffset now) => LastPolledAt = now;

    public void Deactivate() => IsActive = false;
}
