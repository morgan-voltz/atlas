using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class VeillePackEnrollmentRepository(AtlasDbContext dbContext) : IVeillePackEnrollmentRepository
{
    public async Task<VeillePackEnrollment?> GetAsync(UserId userId, VeillePackId packId, CancellationToken ct = default) =>
        await dbContext.VeillePackEnrollments
            .FirstOrDefaultAsync(enrollment => enrollment.UserId == userId && enrollment.PackId == packId, ct);

    public async Task<IReadOnlyList<VeillePackEnrollment>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.VeillePackEnrollments
            .Where(enrollment => enrollment.UserId == userId)
            .OrderByDescending(enrollment => enrollment.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(VeillePackEnrollment enrollment, CancellationToken ct = default) =>
        await dbContext.VeillePackEnrollments.AddAsync(enrollment, ct);

    public void Update(VeillePackEnrollment enrollment) => dbContext.VeillePackEnrollments.Update(enrollment);
}
