using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

public interface IVeillePackEnrollmentRepository
{
    Task<VeillePackEnrollment?> GetAsync(UserId userId, VeillePackId packId, CancellationToken ct = default);

    Task<IReadOnlyList<VeillePackEnrollment>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(VeillePackEnrollment enrollment, CancellationToken ct = default);

    void Update(VeillePackEnrollment enrollment);
}
