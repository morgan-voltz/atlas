namespace Atlas.Domain.Companies.Attachments;

/// <summary>
/// Document (acte ou bilan) déposé au RNE par une entreprise et téléchargeable via l'INPI (F-013).
/// Modèle de lecture : non persisté côté Atlas (proxy direct).
/// </summary>
/// <param name="Id">Identifiant INPI du document (utilisé pour le téléchargement).</param>
/// <param name="Type">Catégorie (acte / bilan / autre).</param>
/// <param name="Name">Libellé lisible (ex. « Statuts à jour », « Bilan 2024 »).</param>
/// <param name="DepositedAt">Date de dépôt si fournie par l'INPI.</param>
/// <param name="SizeBytes">Taille en octets si fournie.</param>
/// <param name="IsConfidential">Bilans confidentiels : <c>true</c> → téléchargement refusé pour le grand public.</param>
public sealed record CompanyAttachment(
    string Id,
    AttachmentType Type,
    string Name,
    DateOnly? DepositedAt,
    long? SizeBytes,
    bool IsConfidential);
