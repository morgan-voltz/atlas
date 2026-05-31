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

    Task<ApiResult<PagedResult<CompanySummaryResponse>>> SearchCompaniesAsync(
        string name,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<ApiResult<CompanyResponse>> GetCompanyBySirenAsync(string siren, CancellationToken ct = default);
}
