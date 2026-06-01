using System.Net;
using System.Net.Sockets;

namespace Atlas.Infrastructure.Veille;

/// <summary>
/// Garde anti-SSRF (audit Lot 1). Détecte les hôtes / adresses IP pointant vers loopback,
/// réseau privé (RFC 1918), link-local et métadonnées cloud (169.254.0.0/16), ainsi que
/// leurs équivalents IPv6 (loopback, link-local, site-local, unique-local fc00::/7).
///
/// Partagée entre deux points de contrôle complémentaires :
/// <list type="bullet">
///   <item>la validation à l'ajout d'une source (<see cref="FeedSubscriptionPolicy"/>), qui ne
///   voit que le nom d'hôte ;</item>
///   <item>la validation au moment de la connexion TCP (ConnectCallback du client HTTP RSS),
///   qui voit l'IP réellement résolue et couvre donc le rebinding DNS et les redirections 3xx —
///   impossibles à filtrer sur le seul nom d'hôte (TOCTOU).</item>
/// </list>
/// </summary>
internal static class PrivateNetworkGuard
{
    /// <summary>
    /// Bloque un hôte littéral connu comme interne. Un nom DNS (non IP) n'est pas résolu ici :
    /// la résolution est validée au moment de la connexion via <see cref="IsBlockedIp"/>.
    /// </summary>
    public static bool IsBlockedHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IPAddress.TryParse(host, out IPAddress? ip) && IsBlockedIp(ip);
    }

    /// <summary>
    /// Bloque une adresse IP appartenant à une plage non routable / interne.
    /// </summary>
    public static bool IsBlockedIp(IPAddress ip)
    {
        ArgumentNullException.ThrowIfNull(ip);

        // Normalise une IPv4 mappée en IPv6 (ex. ::ffff:127.0.0.1) vers sa forme IPv4.
        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        if (IPAddress.IsLoopback(ip) || ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal)
        {
            return true;
        }

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] bytes = ip.GetAddressBytes();
            return bytes[0] switch
            {
                0 => true,                                // 0.0.0.0/8
                10 => true,                               // 10.0.0.0/8
                127 => true,                              // 127.0.0.0/8 (doublon défensif d'IsLoopback)
                169 when bytes[1] == 254 => true,         // 169.254.0.0/16 (link-local + métadonnées cloud)
                172 when (bytes[1] & 0xF0) == 16 => true, // 172.16.0.0/12
                192 when bytes[1] == 168 => true,         // 192.168.0.0/16
                _ => false,
            };
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            byte[] bytes = ip.GetAddressBytes();
            // fc00::/7 — adresses unique-local IPv6 (équivalent des plages privées IPv4).
            if ((bytes[0] & 0xFE) == 0xFC)
            {
                return true;
            }
        }

        return false;
    }
}
