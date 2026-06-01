using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Atlas.Api.Security;

/// <summary>
/// Wiring DI des composants de sécurité applicative (Lot 2b audit) :
/// CORS strict (origines explicites, jamais <c>*</c>) et rate limiting (global + auth-strict).
/// </summary>
internal static class ApiSecurityServiceCollectionExtensions
{
    /// <summary>
    /// Configure CORS et le rate limiter ASP.NET Core.
    /// Hors Development, exige au moins une origine CORS (échec au démarrage sinon).
    /// </summary>
    public static IServiceCollection AddApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // ── CORS ───────────────────────────────────────────────────────────────────────
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .Validate(
                opts => environment.IsDevelopment() || opts.AllowedOrigins.Count > 0,
                "Cors:AllowedOrigins doit contenir au moins une origine hors Development (jamais '*').")
            .ValidateOnStart();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.DefaultPolicyName, policy =>
            {
                CorsOptions corsOptions =
                    configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
                if (corsOptions.AllowedOrigins.Count > 0)
                {
                    policy.WithOrigins([.. corsOptions.AllowedOrigins])
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            });
        });

        // ── Rate limiting ─────────────────────────────────────────────────────────────
        services.AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.SectionName))
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            RateLimitOptions limits =
                configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions();

            // Politique globale : par adresse IP (X-Forwarded-For derrière un proxy à configurer).
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.Global.PermitLimit,
                        Window = TimeSpan.FromSeconds(limits.Global.WindowSeconds),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));

            // Politique stricte sur les endpoints d'authentification (attaque par force brute).
            options.AddPolicy(RateLimitOptions.AuthStrictPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.AuthStrict.PermitLimit,
                        Window = TimeSpan.FromSeconds(limits.AuthStrict.WindowSeconds),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));

            // Politique dédiée aux endpoints coûteux (audit Lot 4 — M3) : bulk download, génération PDF.
            // Protège le quota INPI de l'utilisateur et le CPU, en plus de la limite globale.
            options.AddPolicy(RateLimitOptions.ExpensivePolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.Expensive.PermitLimit,
                        Window = TimeSpan.FromSeconds(limits.Expensive.WindowSeconds),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext context)
    {
        // Utilisateur authentifié : partition par sub (limite par compte, pas par IP).
        string? sub = context.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(sub))
        {
            return $"user:{sub}";
        }

        // Sinon : partition par IP cliente. (Derrière reverse proxy, configurer
        // ForwardedHeaders en amont pour que RemoteIpAddress reflète le client réel.)
        return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }
}
