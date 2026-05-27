namespace Atlas.Application.Users;

/// <summary>
/// Jetons émis lors d'une authentification réussie. Le <see cref="RefreshToken"/> en clair est destiné
/// à être déposé dans un cookie httpOnly par la couche API ; il n'est jamais persisté en clair.
/// </summary>
public sealed record AuthTokensDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken);
