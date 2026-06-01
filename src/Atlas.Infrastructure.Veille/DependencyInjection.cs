using System.Net;
using System.Net.Sockets;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Veille.Deduplication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Veille;

public static class DependencyInjection
{
    public static IServiceCollection AddVeilleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<VeilleOptions>(configuration.GetSection(VeilleOptions.SectionName));

        services.AddHttpClient<IExternalContentSource, RssFeedProvider>(ConfigureClient)
            // Anti-SSRF (audit Lot 1) : chaque connexion TCP — fetch initial, redirection 3xx ou
            // résolution DNS — est validée contre les plages internes. Couvre le rebinding DNS et
            // les redirections, que le seul filtrage du nom d'hôte ne peut pas attraper (TOCTOU).
            .ConfigurePrimaryHttpMessageHandler(CreateSsrfSafeHandler)
            // Résilience (audit Lot 2) : retry transitoire + circuit breaker. Chaque tentative repasse
            // par le ConnectCallback ci-dessus, donc le retry reste protégé contre le SSRF.
            .AddStandardResilienceHandler().Configure(ConfigureResilience);

        services.AddSingleton<IFeedSubscriptionPolicy, FeedSubscriptionPolicy>();
        services.AddSingleton<IDeduplicationPolicy, DeduplicationPolicy>();

        return services;
    }

    private static void ConfigureClient(IServiceProvider provider, HttpClient client)
    {
        VeilleOptions options = provider.GetRequiredService<IOptions<VeilleOptions>>().Value;
        client.Timeout = Timeout.InfiniteTimeSpan; // géré par le pipeline de résilience
        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, IServiceProvider provider)
    {
        int seconds = provider.GetRequiredService<IOptions<VeilleOptions>>().Value.HttpTimeoutSeconds;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(seconds);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(seconds * 4);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(seconds * 2);
    }

    private static SocketsHttpHandler CreateSsrfSafeHandler() => new()
    {
        // Les redirections restent suivies (de nombreux flux légitimes en usent), mais chaque
        // connexion résultante repasse par ConnectCallback : aucune ne peut atteindre une IP interne.
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 5,
        ConnectCallback = SsrfSafeConnectAsync,
    };

    private static async ValueTask<Stream> SsrfSafeConnectAsync(
        SocketsHttpConnectionContext context,
        CancellationToken ct)
    {
        DnsEndPoint endpoint = context.DnsEndPoint;

        IPAddress[] resolved = await Dns.GetHostAddressesAsync(endpoint.Host, ct);
        IPAddress[] allowed = Array.FindAll(resolved, ip => !PrivateNetworkGuard.IsBlockedIp(ip));
        if (allowed.Length == 0)
        {
            throw new HttpRequestException(
                $"Connexion refusée : l'hôte « {endpoint.Host} » résout vers une adresse interne (anti-SSRF).");
        }

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            // On se connecte uniquement aux adresses validées (pas de re-résolution => pas de TOCTOU).
            await socket.ConnectAsync(allowed, endpoint.Port, ct);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
