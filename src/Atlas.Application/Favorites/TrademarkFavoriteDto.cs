namespace Atlas.Application.Favorites;

/// <summary>DTO d'affichage pour une marque favorite (F-018).</summary>
public sealed record TrademarkFavoriteDto(string DepositNumber, string? Name, DateTimeOffset AddedAt);
