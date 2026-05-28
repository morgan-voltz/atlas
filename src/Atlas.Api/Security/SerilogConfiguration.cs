using System.Globalization;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Atlas.Api.Security;

/// <summary>
/// Configuration Serilog pour Atlas (Lot 2b audit). Log structuré console JSON-friendly
/// + masquage proactif des propriétés sensibles (mot de passe, token, clés, identifiants INPI)
/// via un <see cref="ILogEventEnricher"/>.
/// </summary>
internal static class SerilogConfiguration
{
    public static void Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.With<SensitiveDataMaskingEnricher>();
    }
}

/// <summary>
/// Masque les valeurs des propriétés structurées dont le nom suggère un secret.
/// Empêche par exemple <c>Log.Information("Connexion {@Credentials}", creds)</c> ou
/// <c>logger.LogError(ex, "...")</c> avec une exception contenant un champ <c>Password</c>
/// de fuiter en clair dans les sinks. Ne remplace pas la règle « ne jamais logger
/// d'<c>InpiCredentials</c> » du CLAUDE.md, mais ajoute une garde de défense en profondeur.
/// </summary>
internal sealed class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private static readonly string[] SensitiveSubstrings =
    [
        "password",
        "passphrase",
        "token",          // access token, refresh token, verification token
        "secret",
        "credential",     // InpiCredentials, etc.
        "privatekey",
        "keybase64",
        "authorization",
    ];

    private const string MaskedValue = "***REDACTED***";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        foreach (KeyValuePair<string, LogEventPropertyValue> property in logEvent.Properties.ToArray())
        {
            if (IsSensitive(property.Key))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, MaskedValue));
            }
        }
    }

    private static bool IsSensitive(string propertyName)
    {
        foreach (string fragment in SensitiveSubstrings)
        {
            if (propertyName.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
