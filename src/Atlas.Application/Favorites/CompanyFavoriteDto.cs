namespace Atlas.Application.Favorites;

/// <summary>DTO d'affichage pour une entreprise favorite (F-017).</summary>
public sealed record CompanyFavoriteDto(string Siren, string? Name, DateTimeOffset AddedAt);
