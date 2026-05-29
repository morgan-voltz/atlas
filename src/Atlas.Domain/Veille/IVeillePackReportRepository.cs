using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// Port d'accès aux signalements de packs de veille (F-049 marketplace, modèle <em>report &amp; review</em>).
/// </summary>
public interface IVeillePackReportRepository
{
    Task<bool> ExistsPendingByReporterAsync(UserId reporterUserId, VeillePackId veillePackId, CancellationToken ct = default);

    Task AddAsync(VeillePackReport report, CancellationToken ct = default);
}
