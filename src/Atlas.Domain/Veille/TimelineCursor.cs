namespace Atlas.Domain.Veille;

/// <summary>
/// Position keyset dans la timeline de veille, ordonnée par <c>(OccurredAt DESC, Id DESC)</c>.
/// <para>
/// La timeline fusionne deux flux (items RSS + événements favoris). <see cref="OccurredAt"/> est la clé
/// de tri commune ; <see cref="Id"/> (Guid de l'item, unique inter-flux) départage les ex-aequo
/// d'horodatage — fréquents en RSS (dates à la minute). Le couple forme un ordre total stable.
/// </para>
/// </summary>
public sealed record TimelineCursor(DateTimeOffset OccurredAt, Guid Id);

/// <summary>
/// Ordre keyset de la timeline. La comparaison des <see cref="Guid"/> suit l'ordre des <c>uuid</c>
/// PostgreSQL (octets big-endian) afin que le filtrage en mémoire coïncide EXACTEMENT avec un
/// <c>ORDER BY id</c> côté base — sinon le keyset pourrait sauter ou dupliquer des éléments.
/// </summary>
public static class TimelineKeyset
{
    /// <summary>Compare deux Guid comme PostgreSQL compare les <c>uuid</c> (lexicographique big-endian).</summary>
    public static int CompareGuid(Guid left, Guid right)
    {
        Span<byte> a = stackalloc byte[16];
        Span<byte> b = stackalloc byte[16];
        left.TryWriteBytes(a, bigEndian: true, out _);
        right.TryWriteBytes(b, bigEndian: true, out _);
        return a.SequenceCompareTo(b);
    }

    /// <summary>
    /// Vrai si l'élément <c>(occurredAt, id)</c> se situe STRICTEMENT après le curseur dans l'ordre
    /// <c>(OccurredAt DESC, Id DESC)</c> — c.-à-d. fait partie de la page suivante.
    /// </summary>
    public static bool IsAfter(DateTimeOffset occurredAt, Guid id, TimelineCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        if (occurredAt < cursor.OccurredAt)
        {
            return true;
        }

        return occurredAt == cursor.OccurredAt && CompareGuid(id, cursor.Id) < 0;
    }
}
