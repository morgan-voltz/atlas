using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.GetBulkDownload;

public sealed record GetBulkDownloadQuery(Guid UserId, Guid JobId)
    : IRequest<Result<BulkDownloadJobDto>>;

public sealed record BulkDownloadJobDto(
    Guid Id,
    IReadOnlyList<string> Sirens,
    string Status,
    string? ArchiveKey,
    string? ErrorMessage,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset ExpiresAt);
