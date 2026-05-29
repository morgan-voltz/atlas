namespace Atlas.Domain.Companies.Attachments;

/// <summary>
/// Contenu binaire d'un attachment téléchargé depuis l'INPI (F-013). Le <see cref="Stream"/>
/// est consommé par l'appelant puis disposé. Le caller est responsable de fermer le stream.
/// </summary>
/// <param name="Stream">Flux binaire du document.</param>
/// <param name="ContentType">Type MIME (typiquement <c>application/pdf</c>).</param>
/// <param name="FileName">Nom de fichier suggéré pour l'attachement HTTP.</param>
public sealed record AttachmentContent(
    Stream Stream,
    string ContentType,
    string FileName);
