using Atlas.Application.Downloads;
using Hangfire;

namespace Atlas.Api.Downloads;

/// <summary>
/// Adapter Hangfire de <see cref="IBulkDownloadEnqueuer"/> — la voie de production.
/// </summary>
internal sealed class HangfireBulkDownloadEnqueuer(IBackgroundJobClient backgroundJobs)
    : IBulkDownloadEnqueuer
{
    public void Enqueue(Guid jobId) =>
        backgroundJobs.Enqueue<BulkDownloadJob>(job => job.RunAsync(jobId));
}
