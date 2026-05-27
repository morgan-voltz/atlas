using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.ApplyVeillePack;

internal sealed class ApplyVeillePackHandler(
    IVeillePackRepository packRepository,
    IVeillePackEnrollmentRepository enrollmentRepository,
    VeillePackEnroller enroller,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApplyVeillePackCommand, Result<ApplyVeillePackResult>>
{
    public async Task<Result<ApplyVeillePackResult>> Handle(
        ApplyVeillePackCommand request,
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
        bool isNew = enrollment is null;
        enrollment ??= VeillePackEnrollment.Create(userId, pack.Id, pack.Version, clock.UtcNow);

        int added = await enroller.BringUpToDateAsync(userId, pack, enrollment, cancellationToken);

        if (isNew)
        {
            await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        }
        else
        {
            enrollmentRepository.Update(enrollment);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ApplyVeillePackResult>.Ok(new ApplyVeillePackResult(pack.Code, pack.Name, added, pack.Version));
    }
}
