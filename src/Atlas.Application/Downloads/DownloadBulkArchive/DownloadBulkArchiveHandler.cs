using Atlas.Domain.Common;
using Atlas.Domain.Downloads;
using Atlas.Domain.Storage;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.DownloadBulkArchive;

internal sealed class DownloadBulkArchiveHandler(
    IBulkDownloadJobRepository repository,
    IFileStorage storage,
    IDateTimeProvider clock) : IRequestHandler<DownloadBulkArchiveQuery, Result<BulkArchiveContent>>
{
    public async Task<Result<BulkArchiveContent>> Handle(DownloadBulkArchiveQuery request, CancellationToken cancellationToken)
    {
        BulkDownloadJob? job =
            await repository.GetByIdAsync(new BulkDownloadJobId(request.JobId), cancellationToken);

        if (job is null || job.UserId != new UserId(request.UserId))
        {
            return Result<BulkArchiveContent>.Fail(BulkDownloadErrors.NotFound);
        }

        if (job.Status != BulkDownloadStatus.Ready || job.ArchiveKey is null)
        {
            return Result<BulkArchiveContent>.Fail(BulkDownloadErrors.NotReady);
        }

        if (job.IsExpired(clock.UtcNow))
        {
            return Result<BulkArchiveContent>.Fail(BulkDownloadErrors.Expired);
        }

        Stream? content = await storage.OpenReadAsync(job.ArchiveKey, cancellationToken);
        if (content is null)
        {
            return Result<BulkArchiveContent>.Fail(BulkDownloadErrors.NotFound);
        }

        return Result<BulkArchiveContent>.Ok(new BulkArchiveContent(content, $"atlas-bulk-{job.Id}.zip"));
    }
}
