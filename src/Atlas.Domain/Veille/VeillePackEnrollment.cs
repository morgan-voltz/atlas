using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Veille;

/// <summary>
/// Inscription d'un utilisateur à un <see cref="VeillePack"/>, avec la version du pack appliquée. Permet de
/// détecter qu'une nouvelle version est disponible et de re-synchroniser. Cf. doc 08 §9.x, F-042.
/// </summary>
public sealed class VeillePackEnrollment : Entity<VeillePackEnrollmentId>
{
    private VeillePackEnrollment()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private VeillePackEnrollment(
        VeillePackEnrollmentId id,
        UserId userId,
        VeillePackId packId,
        int appliedVersion,
        DateTimeOffset now)
        : base(id)
    {
        UserId = userId;
        PackId = packId;
        AppliedVersion = appliedVersion;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public UserId UserId { get; private set; }

    public VeillePackId PackId { get; private set; }

    /// <summary>Version du pack au moment de la dernière application/synchronisation.</summary>
    public int AppliedVersion { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static VeillePackEnrollment Create(UserId userId, VeillePackId packId, int version, DateTimeOffset now) =>
        new(VeillePackEnrollmentId.New(), userId, packId, version, now);

    /// <summary>Marque l'inscription comme synchronisée sur la version donnée.</summary>
    public void MarkSynced(int version, DateTimeOffset now)
    {
        AppliedVersion = version;
        UpdatedAt = now;
    }
}
