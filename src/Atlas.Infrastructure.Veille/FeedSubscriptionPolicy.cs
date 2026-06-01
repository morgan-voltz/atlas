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

        // Anti-SSRF (audit Lot 2a, renforcé Lot 1) : rejette les hôtes loopback / réseau privé /
        // link-local / métadonnées cloud sur les URL fournies par l'utilisateur (F-043). Le cas d'un
        // nom DNS qui résout vers une IP interne (rebinding/TOCTOU) et les redirections 3xx sont
        // couverts en complément par le ConnectCallback du client HTTP RSS (cf. PrivateNetworkGuard).
        if (PrivateNetworkGuard.IsBlockedHost(uri.Host))
        {
            return false;
        }

        string host = uri.Host;
        return !_options.BlockedHostFragments.Any(
            fragment => host.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
