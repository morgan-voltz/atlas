using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Domain.Veille;

/// <summary>
/// Signalement d'un <see cref="VeillePack"/> publié par un utilisateur (F-049 marketplace).
/// Fonctionne en *report &amp; review* (modération a posteriori) : un signalement positionne le pack
/// en revue pour examen ; l'examen est ensuite acté via <see cref="MarkReviewed"/>.
/// </summary>
public sealed class VeillePackReport : Entity<VeillePackReportId>
{
    public const int MaxReasonLength = 500;

    private VeillePackReport()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private VeillePackReport(
        VeillePackReportId id,
        VeillePackId veillePackId,
        UserId reporterUserId,
        string reason,
        DateTimeOffset createdAt)
        : base(id)
    {
        VeillePackId = veillePackId;
        ReporterUserId = reporterUserId;
        Reason = reason;
        Status = VeillePackReportStatus.Pending;
        CreatedAt = createdAt;
    }

    public VeillePackId VeillePackId { get; private set; }

    public UserId ReporterUserId { get; private set; }

    /// <summary>Motif libre fourni par le signalant (1 à 500 caractères).</summary>
    public string Reason { get; private set; } = null!;

    public VeillePackReportStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public static Result<VeillePackReport> Create(
        VeillePackId veillePackId,
        UserId reporterUserId,
        string reason,
        DateTimeOffset now)
    {
        string trimmed = (reason ?? string.Empty).Trim();
        if (trimmed.Length is 0 or > MaxReasonLength)
        {
            return Result<VeillePackReport>.Fail(
                VeilleErrors.InvalidVeillePackReport("motif requis (1 à 500 caractères)."));
        }

        return Result<VeillePackReport>.Ok(new VeillePackReport(
            VeillePackReportId.New(),
            veillePackId,
            reporterUserId,
            trimmed,
            now));
    }

    /// <summary>Marque le signalement comme examiné. <paramref name="removed"/> indique si le pack a été retiré.</summary>
    public void MarkReviewed(bool removed, DateTimeOffset now)
    {
        Status = removed
            ? VeillePackReportStatus.ReviewedRemoved
            : VeillePackReportStatus.ReviewedNoAction;
        ReviewedAt = now;
    }
}
