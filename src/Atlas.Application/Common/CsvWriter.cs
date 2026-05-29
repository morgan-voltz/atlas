using System.Globalization;
using System.Text;

namespace Atlas.Application.Common;

/// <summary>
/// Helper d'écriture CSV (F-021). Format **RFC 4180** : champs entre guillemets si nécessaire,
/// guillemets internes doublés, séparateur <c>,</c>, retour ligne <c>CRLF</c>. Encodage UTF-8 avec
/// BOM pour qu'Excel respecte les caractères accentués au double-clic.
/// </summary>
public static class CsvWriter
{
    private static readonly byte[] Utf8Bom = [0xEF, 0xBB, 0xBF];

    /// <summary>
    /// Sérialise une liste d'items en CSV avec une ligne d'en-tête et N colonnes nommées.
    /// </summary>
    public static byte[] WriteToBytes<T>(
        IEnumerable<T> items,
        IReadOnlyList<CsvColumn<T>> columns)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(columns);

        using var ms = new MemoryStream();
        ms.Write(Utf8Bom);

        using var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            NewLine = "\r\n",
        };

        // Header
        writer.WriteLine(string.Join(",", columns.Select(c => Escape(c.Header))));

        // Rows
        foreach (T item in items)
        {
            string line = string.Join(",", columns.Select(c => Escape(FormatValue(c.Selector(item)))));
            writer.WriteLine(line);
        }

        writer.Flush();
        return ms.ToArray();
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTimeOffset dto => dto.ToString("o", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static string Escape(string field)
    {
        bool needsQuoting = field.IndexOfAny([',', '"', '\n', '\r']) >= 0;
        if (!needsQuoting)
        {
            return field;
        }

        return $"\"{field.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}

/// <summary>Définition d'une colonne CSV : en-tête + extraction de la valeur depuis un item.</summary>
public sealed record CsvColumn<T>(string Header, Func<T, object?> Selector);
