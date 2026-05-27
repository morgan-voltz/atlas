using System.Globalization;
using System.Numerics;
using System.Text;

namespace Atlas.Domain.Veille;

/// <summary>
/// Empreinte floue 64 bits (SimHash) pour la déduplication intelligente (F-045). Deux textes proches
/// produisent des empreintes dont la distance de Hamming est faible. Le hash de token utilisé est un
/// FNV-1a 64 bits déterministe : <see cref="string.GetHashCode()"/> est randomisé par process et casserait
/// le clustering entre redémarrages. Helper pur, sans dépendance hors BCL (cf. règle de dépendance Domain).
/// </summary>
public static class SimHash
{
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

    /// <summary>Empreinte SimHash 64 bits du texte (titre + extrait), après normalisation et tokenisation.</summary>
    public static long Compute(string? text)
    {
        int[] weights = new int[64];

        foreach (string token in Tokenize(text))
        {
            ulong tokenHash = Fnv1a(token);
            for (int bit = 0; bit < 64; bit++)
            {
                if ((tokenHash & (1UL << bit)) != 0)
                {
                    weights[bit]++;
                }
                else
                {
                    weights[bit]--;
                }
            }
        }

        ulong fingerprint = 0;
        for (int bit = 0; bit < 64; bit++)
        {
            if (weights[bit] > 0)
            {
                fingerprint |= 1UL << bit;
            }
        }

        return unchecked((long)fingerprint);
    }

    /// <summary>Distance de Hamming entre deux empreintes (nombre de bits différents, 0 à 64).</summary>
    public static int HammingDistance(long a, long b) =>
        BitOperations.PopCount(unchecked((ulong)(a ^ b)));

    private static IEnumerable<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var builder = new StringBuilder(text.Length);
        foreach (char rune in text.Normalize(NormalizationForm.FormD))
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(rune);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(rune))
            {
                builder.Append(char.ToLowerInvariant(rune));
            }
            else if (builder.Length > 0)
            {
                yield return builder.ToString();
                builder.Clear();
            }
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }

    private static ulong Fnv1a(string token)
    {
        ulong hash = FnvOffsetBasis;
        foreach (byte b in Encoding.UTF8.GetBytes(token))
        {
            hash ^= b;
            hash *= FnvPrime;
        }

        return hash;
    }
}
