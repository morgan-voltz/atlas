namespace Atlas.Api.Security;

/// <summary>
/// Paramètres de rate limiting (Lot 2b audit). Lus depuis la section "RateLimit".
/// Deux niveaux : global (toutes les requêtes) et "auth-strict" (endpoints sensibles
/// /auth/login, /auth/register, /auth/refresh, /auth/2fa, /auth/password/forgot).
/// </summary>
internal sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public const string AuthStrictPolicy = "auth-strict";

    /// <summary>Endpoints coûteux (audit Lot 4 — M3) : bulk download, génération PDF (appels INPI + CPU).</summary>
    public const string ExpensivePolicy = "expensive";

    public WindowOptions Global { get; set; } = new() { PermitLimit = 100, WindowSeconds = 60 };

    public WindowOptions AuthStrict { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    public WindowOptions Expensive { get; set; } = new() { PermitLimit = 20, WindowSeconds = 60 };

    public sealed class WindowOptions
    {
        public int PermitLimit { get; set; }

        public int WindowSeconds { get; set; }
    }
}
