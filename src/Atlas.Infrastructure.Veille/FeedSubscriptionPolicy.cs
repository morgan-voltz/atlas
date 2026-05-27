using Atlas.Domain.Veille;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Veille;

/// <summary>
/// Implémentation de <see cref="IFeedSubscriptionPolicy"/> adossée à <see cref="VeilleOptions"/> (F-043).
/// La modération repose sur une liste de fragments d'hôtes interdits, comparés à l'hôte de l'URL.
/// </summary>
internal sealed class FeedSubscriptionPolicy(IOptions<VeilleOptions> options) : IFeedSubscriptionPolicy
{
    private readonly VeilleOptions _options = options.Value;

    public int? MaxSubscriptionsPerUser =>
        _options.MaxSubscriptionsPerUser is { } max && max > 0 ? max : null;

    public TimeSpan UserFeedPollingInterval => TimeSpan.FromMinutes(_options.UserFeedPollingMinutes);

    public bool IsUrlAllowed(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            // URL non parsable : on laisse la validation de FeedSource.Create rejeter avec une erreur dédiée.
            return true;
        }

        string host = uri.Host;
        return !_options.BlockedHostFragments.Any(
            fragment => host.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
