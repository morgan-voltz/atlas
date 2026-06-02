namespace Atlas.Shared.Result;

/// <summary>
/// Page de résultats en pagination keyset (curseur opaque) — alternative à <see cref="PagedResult{T}"/>
/// pour les flux où la pagination par OFFSET est coûteuse ou plafonnée (timeline de veille).
/// <para>
/// <see cref="NextCursor"/> est un jeton opaque à renvoyer tel quel pour obtenir la page suivante ;
/// <c>null</c> signifie qu'il n'y a plus d'éléments. Pas de total : un feed à défilement infini n'en a pas
/// besoin et le calcul du total est précisément l'un des coûts que le keyset évite.
/// </para>
/// </summary>
public sealed record CursorPage<T>(
    IReadOnlyList<T> Items,
    string? NextCursor)
{
    public bool HasMore => NextCursor is not null;
}
