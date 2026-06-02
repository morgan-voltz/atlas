using System.Buffers.Text;
using System.Globalization;
using System.Text;
using Atlas.Domain.Veille;

namespace Atlas.Application.Veille.GetTimeline;

/// <summary>
/// Encode/décode le curseur keyset de la timeline en jeton opaque URL-safe.
/// Format interne : <c>{instantUtcTicks}:{idGuidN}</c>. Seul l'instant UTC est conservé : la comparaison
/// de <see cref="DateTimeOffset"/> porte sur l'instant, pas sur le décalage horaire.
/// </summary>
internal static class TimelineCursorCodec
{
    public static string Encode(TimelineCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        string raw = $"{cursor.OccurredAt.UtcTicks.ToString(CultureInfo.InvariantCulture)}:{cursor.Id:N}";
        return Base64Url.EncodeToString(Encoding.UTF8.GetBytes(raw));
    }

    /// <summary>Décode un jeton ; renvoie <c>null</c> si absent ou illisible (traité comme première page).</summary>
    public static TimelineCursor? Decode(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            string raw = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(token));
            int separator = raw.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                return null;
            }

            if (!long.TryParse(raw.AsSpan(0, separator), NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)
                || !Guid.TryParseExact(raw.AsSpan(separator + 1), "N", out Guid id))
            {
                return null;
            }

            return new TimelineCursor(new DateTimeOffset(ticks, TimeSpan.Zero), id);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
