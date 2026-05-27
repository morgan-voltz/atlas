using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Veille.GetMyVeillePacks;

internal sealed class GetMyVeillePacksHandler(
    IVeillePackEnrollmentRepository enrollmentRepository,
    IVeillePackRepository packRepository)
    : IRequestHandler<GetMyVeillePacksQuery, Result<IReadOnlyList<MyVeillePackDto>>>
{
    public async Task<Result<IReadOnlyList<MyVeillePackDto>>> Handle(
        GetMyVeillePacksQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

        IReadOnlyList<VeillePackEnrollment> enrollments =
            await enrollmentRepository.GetByUserAsync(userId, cancellationToken);

        var dtos = new List<MyVeillePackDto>(enrollments.Count);
        foreach (VeillePackEnrollment enrollment in enrollments)
        {
            VeillePack? pack = await packRepository.GetByIdAsync(enrollment.PackId, cancellationToken);
            if (pack is null || !pack.IsActive)
            {
                continue;
            }

            dtos.Add(new MyVeillePackDto(
                pack.Code,
                pack.Name,
                enrollment.AppliedVersion,
                pack.Version,
                pack.Version > enrollment.AppliedVersion,
                enrollment.CreatedAt));
        }

        return Result<IReadOnlyList<MyVeillePackDto>>.Ok(dtos);
    }
}
