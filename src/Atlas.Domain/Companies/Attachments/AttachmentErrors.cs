using Atlas.Domain.Common;

namespace Atlas.Domain.Companies.Attachments;

public static class AttachmentErrors
{
    public static DomainError NotFound { get; } = new NotFoundError();

    public static DomainError Confidential { get; } = new ConfidentialError();

    private sealed record NotFoundError()
        : DomainError("companies.attachment_not_found", "Le document demandé est introuvable.");

    private sealed record ConfidentialError()
        : DomainError("companies.attachment_confidential", "Ce bilan est déclaré confidentiel — téléchargement refusé.");
}
