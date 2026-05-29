using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.DownloadBulkArchive;

public sealed record DownloadBulkArchiveQuery(Guid UserId, Guid JobId)
    : IRequest<Result<BulkArchiveContent>>;

public sealed record BulkArchiveContent(Stream Stream, string FileName);
