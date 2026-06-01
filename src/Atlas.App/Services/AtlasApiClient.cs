using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Atlas.Domain.Companies;
using Atlas.Shared.Result;

namespace Atlas.App.Services;

/// <summary>
/// Unique point d'entrée du client Uno vers le backend (ADR-002 : topologie « client pur de l'API »).
/// Toute interaction réseau passe par ici ; aucune logique métier sensible ni aucun secret ne vit côté
/// client (code décompilable, surtout la tête WASM). Le client ne référence que <c>Atlas.Domain</c> et
/// <c>Atlas.Shared</c> — jamais <c>Atlas.Infrastructure.*</c> (verrouillé par Atlas.Architecture.Tests).
///
/// Stub U1/U2 : la signature et la construction de requête sont posées ; le mapping de la réponse vers
/// un DTO client et l'auth (access token en mémoire + refresh cookie/secure storage, ADR-010) seront
/// câblés à l'étape U2/U4 (cf. docs/15 §7).
/// </summary>
public sealed class AtlasApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Récupère la fiche d'une entreprise par son SIREN. Le <see cref="Siren"/> (value object validé
    /// côté domaine) est la seule entrée acceptée — pas de <c>string</c> nu (cf. CLAUDE.md).
    /// </summary>
    public async Task<Result<string>> GetCompanyRawAsync(Siren siren, CancellationToken ct = default)
    {
        using var response = await _httpClient
            .GetAsync($"companies/{siren.Value}", ct)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Fail(ApiErrors.RequestFailed((int)response.StatusCode));
        }

        string body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return Result<string>.Ok(body);
    }
}
