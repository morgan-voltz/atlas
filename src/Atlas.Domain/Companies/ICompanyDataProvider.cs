using Atlas.Domain.Companies.Attachments;
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

    /// <summary>Liste des actes et bilans déposés par une entreprise (F-013).</summary>
    Task<Result<IReadOnlyList<CompanyAttachment>>> GetAttachmentsAsync(
        Siren siren,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);

    /// <summary>Téléchargement binaire d'un document (F-013). Le caller doit disposer le <see cref="AttachmentContent.Stream"/>.</summary>
    Task<Result<AttachmentContent>> DownloadAttachmentAsync(
        Siren siren,
        string attachmentId,
        InpiAccessCredentials credentials,
        CancellationToken ct = default);
}
