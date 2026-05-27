using Atlas.Application.Veille.ApplyVeillePack;
using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.SyncVeillePack;

internal sealed class SyncVeillePackHandler(
    IVeillePackRepository packRepository,
    IVeillePackEnrollmentRepository enrollmentRepository,
    VeillePackEnroller enroller,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SyncVeillePackCommand, Result<ApplyVeillePackResult>>
{
    public async Task<Result<ApplyVeillePackResult>> Handle(
        SyncVeillePackCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        string code = (request.PackCode ?? string.Empty).Trim().ToLowerInvariant();

        VeillePack? pack = await packRepository.GetByCodeAsync(code, cancellationToken);
        if (pack is null || !pack.IsActive)
        {
            return Result<ApplyVeillePackResult>.Fail(VeilleErrors.VeillePackNotFound);
        }

        VeillePackEnrollment? enrollment = await enrollmentRepository.GetAsync(userId, pack.Id, cancellationToken);
        if (enrollment is null)
        {
            return Result<ApplyVeillePackResult>.Fail(VeilleErrors.VeillePackNotEnrolled);
        }

        int added = await enroller.BringUpToDateAsync(userId, pack, enrollment, cancellationToken);
        enrollmentRepository.Update(enrollment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ApplyVeillePackResult>.Ok(new ApplyVeillePackResult(pack.Code, pack.Name, added, pack.Version));
    }
}
