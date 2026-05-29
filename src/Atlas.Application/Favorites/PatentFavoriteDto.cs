namespace Atlas.Application.Favorites;

/// <summary>DTO d'affichage pour un brevet favori (F-018).</summary>
public sealed record PatentFavoriteDto(string PublicationNumber, string? Title, DateTimeOffset AddedAt);
