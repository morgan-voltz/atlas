using Atlas.Maui.Models;
using Atlas.Shared.Result;

namespace Atlas.Maui.Services;

/// <summary>
/// Client HTTP de l'API Atlas. Seul point d'entrée du client mobile vers le backend (cf. CLAUDE.md :
/// Atlas.Maui ne parle jamais directement à l'infrastructure).
/// </summary>
public interface IAtlasApiClient
{
    Task<bool> LoginAsync(string email, string password, CancellationToken ct = default);

    Task LogoutAsync(CancellationToken ct = default);

    Task<bool> IsAuthenticatedAsync();

    Task<CompanyResponse?> GetCompanyBySirenAsync(string siren, CancellationToken ct = default);

    Task<PagedResult<CompanySummaryResponse>?> SearchCompaniesAsync(
        string name,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<SearchHistoryEntryResponse>> GetSearchHistoryAsync(CancellationToken ct = default);

    Task<AccessibilityPreferencesResponse?> GetAccessibilityPreferencesAsync(CancellationToken ct = default);

    Task<bool> UpdateAccessibilityPreferencesAsync(
        AccessibilityPreferencesResponse preferences,
        CancellationToken ct = default);
}
