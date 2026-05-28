using System.Net;
using System.Net.Sockets;
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

        // Anti-SSRF (audit Lot 2a) : rejette les hôtes loopback / réseau privé / link-local /
        // métadonnées cloud (169.254.0.0/16) sur les URL fournies par l'utilisateur (F-043).
        if (IsBlockedNetworkHost(uri))
        {
            return false;
        }

        string host = uri.Host;
        return !_options.BlockedHostFragments.Any(
            fragment => host.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Bloque les hôtes pointant vers loopback / réseau privé / link-local. Limite assumée : la
    /// résolution DNS d'un nom d'hôte n'est pas effectuée ici (TOCTOU) ; un filtre au moment du
    /// fetch HTTP la complétera (P2 follow-up audit 4b).
    /// </summary>
    private static bool IsBlockedNetworkHost(Uri uri)
    {
        string host = uri.Host;
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!IPAddress.TryParse(host, out IPAddress? ip))
        {
            // Nom DNS : autorisé ici (résolution + filtre au fetch en suivi).
            return false;
        }

        if (IPAddress.IsLoopback(ip) || ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal)
        {
            return true;
        }

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] bytes = ip.GetAddressBytes();
            // 10.0.0.0/8
            if (bytes[0] == 10)
            {
                return true;
            }
            // 172.16.0.0/12
            if (bytes[0] == 172 && (bytes[1] & 0xF0) == 16)
            {
                return true;
            }
            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }
            // 169.254.0.0/16 (link-local + métadonnées cloud AWS/GCP)
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                return true;
            }
        }

        return false;
    }
}
