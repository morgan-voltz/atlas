using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.GetCompanyAttachments;

public sealed record GetCompanyAttachmentsQuery(Guid UserId, string Siren)
    : IRequest<Result<IReadOnlyList<CompanyAttachmentDto>>>;

public sealed record CompanyAttachmentDto(
    string Id,
    string Type,
    string Name,
    DateOnly? DepositedAt,
    long? SizeBytes,
    bool IsConfidential);
