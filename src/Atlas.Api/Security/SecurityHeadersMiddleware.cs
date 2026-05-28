namespace Atlas.Api.Security;

/// <summary>
/// Middleware d'en-têtes de sécurité (Lot 2b audit).
/// API JSON pure → CSP très restrictive (default-src 'none'), pas d'iframe (X-Frame-Options DENY),
/// HSTS géré séparément par UseHsts() builtin (prod uniquement).
/// </summary>
internal sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        IHeaderDictionary headers = context.Response.Headers;

        // Empêche le sniffing MIME (force le navigateur à respecter Content-Type).
        headers["X-Content-Type-Options"] = "nosniff";

        // Évite la fuite d'URLs sensibles entre origines.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // L'API n'est pas censée être iframée — clickjacking impossible.
        headers["X-Frame-Options"] = "DENY";

        // CSP très restrictive : l'API ne renvoie que du JSON, jamais de HTML/JS exécutable.
        headers["Content-Security-Policy"] =
            "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

        // Refuse l'accès aux APIs sensibles du navigateur depuis cette origine.
        headers["Permissions-Policy"] =
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

        return _next(context);
    }
}

internal static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
