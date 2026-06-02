using System;
using System.Text.Json;

namespace Atlas.App.Services;

/// <summary>
/// Lecture (sans validation cryptographique) de claims d'un access token JWT, côté client. La signature
/// est vérifiée par l'API à chaque requête ; ici on ne fait qu'extraire des informations d'affichage
/// (ex. l'e-mail pour la confirmation de suppression de compte). Aucune décision de sécurité ne repose
/// dessus. Utilise <see cref="JsonDocument"/> (sans réflexion → compatible trimming WASM).
/// </summary>
internal static class JwtReader
{
    /// <summary>Extrait le claim <c>email</c> du payload, ou <c>null</c> si absent / token mal formé.</summary>
    public static string? ReadEmail(string? token) => ReadClaim(token, "email");

    private static string? ReadClaim(string? token, string claim)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        string[] parts = token.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            byte[] payload = DecodeBase64Url(parts[1]);
            using JsonDocument doc = JsonDocument.Parse(payload);
            return doc.RootElement.TryGetProperty(claim, out JsonElement value) ? value.GetString() : null;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }

    private static byte[] DecodeBase64Url(string segment)
    {
        string padded = segment.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }
}
