using Atlas.Domain.Downloads;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Downloads.GetBulkDownload;

internal sealed class GetBulkDownloadHandler(IBulkDownloadJobRepository repository)
    : IRequestHandler<GetBulkDownloadQuery, Result<BulkDownloadJobDto>>
{
    public async Task<Result<BulkDownloadJobDto>> Handle(GetBulkDownloadQuery request, CancellationToken cancellationToken)
    {
        BulkDownloadJob? job =
            await repository.GetByIdAsync(new BulkDownloadJobId(request.JobId), cancellationToken);

        if (job is null || job.UserId != new UserId(request.UserId))
        {
            return Result<BulkDownloadJobDto>.Fail(BulkDownloadErrors.NotFound);
        }

        return Result<BulkDownloadJobDto>.Ok(new BulkDownloadJobDto(
            job.Id.Value,
            job.SirenList,
            job.Status.ToString(),
            job.ArchiveKey,
            job.ErrorMessage,
            job.RequestedAt,
            job.CompletedAt,
            job.ExpiresAt));
    }
}
