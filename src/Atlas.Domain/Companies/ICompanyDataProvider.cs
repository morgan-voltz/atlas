using Atlas.Domain.Inpi;
using Atlas.Shared.Result;

namespace Atlas.Domain.Companies;

/// <summary>
/// Lecture des données entreprises depuis la source externe (INPI RNE).
/// L'authentification est assurée à partir des identifiants INPI de l'utilisateur (modèle multi-tenant).
/// </summary>
public interface ICompanyDataProvider
{
    Task<Result<UniteLegale>> GetBySirenAsync(
        Siren siren,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);

    Task<Result<PagedResult<CompanySummary>>> SearchByNameAsync(
        CompanySearchQuery query,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);
}
