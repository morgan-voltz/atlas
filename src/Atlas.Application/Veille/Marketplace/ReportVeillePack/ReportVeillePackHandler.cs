using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.Marketplace.ReportVeillePack;

internal sealed class ReportVeillePackHandler(
    IVeillePackRepository packRepository,
    IVeillePackReportRepository reportRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReportVeillePackCommand, Result>
{
    public async Task<Result> Handle(ReportVeillePackCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        string normalizedCode = (request.Code ?? string.Empty).Trim().ToLowerInvariant();

        VeillePack? pack = await packRepository.GetByCodeAsync(normalizedCode, cancellationToken);
        if (pack is null)
        {
            return Result.Fail(VeilleErrors.VeillePackNotFound);
        }

        if (pack.Visibility != VeillePackVisibility.Public)
        {
            return Result.Fail(VeilleErrors.VeillePackNotPublic);
        }

        if (await reportRepository.ExistsPendingByReporterAsync(userId, pack.Id, cancellationToken))
        {
            // Idempotence : un signalement Pending existe déjà pour ce (reporter, pack).
            return Result.Ok();
        }

        Result<VeillePackReport> created = VeillePackReport.Create(
            pack.Id, userId, request.Reason ?? string.Empty, clock.UtcNow);
        if (created.IsFailure)
        {
            return Result.Fail(created.Error!);
        }

        await reportRepository.AddAsync(created.Value!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
