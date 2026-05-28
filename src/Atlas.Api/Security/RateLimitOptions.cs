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

    public WindowOptions Global { get; set; } = new() { PermitLimit = 100, WindowSeconds = 60 };

    public WindowOptions AuthStrict { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    public sealed class WindowOptions
    {
        public int PermitLimit { get; set; }

        public int WindowSeconds { get; set; }
    }
}
