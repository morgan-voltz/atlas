using System.Security.Claims;
using System.Text.Json;

namespace Atlas.Web.Client.Services;

/// <summary>
/// Lit (sans valider) les claims du payload d'un JWT pour alimenter l'état d'authentification côté UI.
/// La VALIDATION (signature, issuer, expiration) est exclusivement le rôle de l'API : ce code part dans
/// le navigateur et ne doit donc embarquer aucune logique de sécurité ni aucune lib de validation lourde.
/// On décode simplement le segment central (base64url) en claims d'affichage / d'autorisation client.
/// </summary>
internal static class JwtPayloadReader
{
    public static IReadOnlyList<Claim> ReadClaims(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return [];
        }

        string[] segments = jwt.Split('.');
        if (segments.Length < 2)
        {
            return [];
        }

        byte[] payload;
        try
        {
            payload = DecodeBase64Url(segments[1]);
        }
        catch (FormatException)
        {
            return [];
        }

        Dictionary<string, JsonElement>? values;
        try
        {
            values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);
        }
        catch (JsonException)
        {
            return [];
        }

        if (values is null)
        {
            return [];
        }

        var claims = new List<Claim>();
        foreach ((string key, JsonElement element) in values)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(key, item.ToString()));
                    }

                    break;
                default:
                    claims.Add(new Claim(key, element.ToString()));
                    break;
            }
        }

        return claims;
    }

    /// <summary>Vrai si le claim <c>exp</c> (epoch secondes) est dans le futur, à <paramref name="now"/> donné.</summary>
    public static bool IsUnexpired(IReadOnlyList<Claim> claims, DateTimeOffset now)
    {
        Claim? exp = claims.FirstOrDefault(c => c.Type == "exp");
        if (exp is null || !long.TryParse(exp.Value, out long seconds))
        {
            // Pas d'exp lisible : on laisse l'API trancher (un 401 déclenchera le refresh).
            return true;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds) > now;
    }

    private static byte[] DecodeBase64Url(string value)
    {
        string padded = value.Replace('-', '+').Replace('_', '/');
        padded = (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded,
        };

        return Convert.FromBase64String(padded);
    }
}
