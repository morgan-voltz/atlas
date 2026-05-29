using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class VeillePackReportRepository(AtlasDbContext dbContext) : IVeillePackReportRepository
{
    public Task<bool> ExistsPendingByReporterAsync(
        UserId reporterUserId,
        VeillePackId veillePackId,
        CancellationToken ct = default) =>
        dbContext.VeillePackReports.AnyAsync(
            report => report.ReporterUserId == reporterUserId
                      && report.VeillePackId == veillePackId
                      && report.Status == VeillePackReportStatus.Pending,
            ct);

    public async Task AddAsync(VeillePackReport report, CancellationToken ct = default) =>
        await dbContext.VeillePackReports.AddAsync(report, ct);
}
