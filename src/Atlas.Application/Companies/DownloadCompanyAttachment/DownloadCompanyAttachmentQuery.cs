using Atlas.Domain.Companies.Attachments;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.DownloadCompanyAttachment;

public sealed record DownloadCompanyAttachmentQuery(Guid UserId, string Siren, string AttachmentId)
    : IRequest<Result<AttachmentContent>>;
