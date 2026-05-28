namespace Atlas.Domain.Favorites;

/// <summary>Changement observé entre deux snapshots d'une entreprise favorite (F-019).</summary>
public sealed record CompanyFavoriteChange(string Field, string? OldValue, string? NewValue);
