using Atlas.Domain.Inpi;
using Atlas.Shared.Result;

namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Lecture des titres de propriété industrielle depuis l'API INPI PI (api-gateway.inpi.fr).
/// L'authentification (XSRF + tokens) est gérée par l'adapter à partir des identifiants INPI de l'utilisateur.
/// </summary>
public interface IIntellectualPropertyProvider
{
    Task<Result<PagedResult<TrademarkSummary>>> SearchTrademarksAsync(
        TrademarkSearchQuery query,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);

    Task<Result<TrademarkDetail>> GetTrademarkAsync(
        DepositNumber depositNumber,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);

    Task<Result<TrademarkImage>> GetTrademarkImageAsync(
        DepositNumber depositNumber,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);

    /// <summary>Notice d'un brevet par numéro de publication (F-015).</summary>
    Task<Result<PatentDetail>> GetPatentByPublicationNumberAsync(
        PublicationNumber publicationNumber,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);
}
