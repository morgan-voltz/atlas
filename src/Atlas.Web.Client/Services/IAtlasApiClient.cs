using Atlas.Shared.Result;
using Atlas.Web.Client.Models;

namespace Atlas.Web.Client.Services;

/// <summary>
/// Client HTTP du web vers <c>Atlas.Api</c>. Consommateur pur de l'API (ADR-002) : aucune logique
/// métier, aucun secret. Porte le bearer en mémoire et rejoue le refresh silencieux sur 401.
/// </summary>
public interface IAtlasApiClient
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default);

    Task LogoutAsync(CancellationToken ct = default);

    /// <summary>Création de compte (F-001 / M7). Renvoie un succès même si l'email doit encore être vérifié.</summary>
    Task<ApiResult> RegisterAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Vérification d'email via le lien d'activation (<c>userId</c> + <c>token</c>).</summary>
    Task<ApiResult> VerifyEmailAsync(string userId, string token, CancellationToken ct = default);

    /// <summary>Défi 2FA : échange le jeton de défi + un code TOTP/secours contre une session (stocke le bearer).</summary>
    Task<ApiResult> Verify2faAsync(string challengeToken, string code, CancellationToken ct = default);

    /// <summary>Renvoi du lien de vérification d'email (réponse uniforme, anti-énumération).</summary>
    Task<ApiResult> ResendVerificationAsync(string email, CancellationToken ct = default);

    /// <summary>Demande de réinitialisation de mot de passe (réponse uniforme, anti-énumération).</summary>
    Task<ApiResult> RequestPasswordResetAsync(string email, CancellationToken ct = default);

    /// <summary>Réinitialise le mot de passe à partir du lien (userId + token) et d'un nouveau mot de passe.</summary>
    Task<ApiResult> ResetPasswordAsync(string userId, string token, string newPassword, CancellationToken ct = default);

    Task<ApiResult<PagedResult<CompanySummaryResponse>>> SearchCompaniesAsync(
        string name,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<ApiResult<CompanyResponse>> GetCompanyBySirenAsync(string siren, CancellationToken ct = default);

    Task<ApiResult<IReadOnlyList<CompanyFavoriteResponse>>> GetMyCompanyFavoritesAsync(CancellationToken ct = default);

    Task<ApiResult> AddCompanyFavoriteAsync(string siren, string? name, CancellationToken ct = default);

    Task<ApiResult> RemoveCompanyFavoriteAsync(string siren, CancellationToken ct = default);

    Task<ApiResult<InpiConnectionStatusResponse>> GetInpiStatusAsync(CancellationToken ct = default);

    Task<ApiResult> ConnectInpiAsync(string username, string password, CancellationToken ct = default);

    Task<ApiResult> DisconnectInpiAsync(CancellationToken ct = default);

    /// <summary>Export RGPD (art. 20) : renvoie le JSON brut prêt à être téléchargé par l'utilisateur.</summary>
    Task<ApiResult<string>> ExportMyDataAsync(CancellationToken ct = default);

    Task<ApiResult> DeleteMyAccountAsync(CancellationToken ct = default);

    /// <summary>Fil de l'Accueil : mouvements des entités suivies (timeline filtrée <c>mentionsFavoritesOnly</c>).
    /// Pagination keyset : <paramref name="cursor"/> null pour la première page, sinon le curseur renvoyé.</summary>
    Task<ApiResult<CursorPage<TimelineItemResponse>>> GetAccueilFeedAsync(string? cursor, int pageSize, CancellationToken ct = default);

    Task<ApiResult> MarkFeedItemReadAsync(Guid id, CancellationToken ct = default);

    /// <summary>Flux de la Veille : contenu éditorial seul (timeline filtrée <c>editorialOnly</c>, doc 12 §6).
    /// Pagination keyset (cf. <see cref="GetAccueilFeedAsync"/>).</summary>
    Task<ApiResult<CursorPage<TimelineItemResponse>>> GetVeilleFeedAsync(string? cursor, int pageSize, CancellationToken ct = default);

    /// <summary>États d'un item de veille (F-044) : lu / favori / archivé. Les champs <c>null</c> restent inchangés.</summary>
    Task<ApiResult> SetFeedItemStateAsync(Guid id, bool? isRead = null, bool? isFavorite = null, bool? isArchived = null, CancellationToken ct = default);
}
