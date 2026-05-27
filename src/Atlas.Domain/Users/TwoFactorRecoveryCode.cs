using Atlas.Domain.Common;

namespace Atlas.Domain.Users;

/// <summary>
/// Code de secours 2FA à usage unique. Seul le hash est persisté ; le clair n'est montré qu'une fois à la génération.
/// </summary>
public sealed class TwoFactorRecoveryCode : Entity<RecoveryCodeId>
{
    private TwoFactorRecoveryCode()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private TwoFactorRecoveryCode(RecoveryCodeId id, UserId userId, string codeHash, DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        CodeHash = codeHash;
        CreatedAt = createdAt;
    }

    public UserId UserId { get; private set; }

    public string CodeHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public bool IsUsed => UsedAt is not null;

    public static TwoFactorRecoveryCode Create(UserId userId, string codeHash, DateTimeOffset now) =>
        new(RecoveryCodeId.New(), userId, codeHash, now);

    public void MarkUsed(DateTimeOffset now)
    {
        UsedAt ??= now;
    }
}
