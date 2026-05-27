using Atlas.Domain.Common;

namespace Atlas.Domain.Users;

/// <summary>
/// Compte rattaché à un <see cref="User"/>. En MVP 1, relation 1-1 (cf. glossaire 4.2).
/// Regroupera à terme la facturation et les souscriptions.
/// </summary>
public sealed class Account : Entity<AccountId>
{
    private Account()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private Account(AccountId id, UserId userId, DateTimeOffset createdAt)
        : base(id)
    {
        UserId = userId;
        CreatedAt = createdAt;
    }

    public UserId UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Account Create(AccountId id, UserId userId, DateTimeOffset now) => new(id, userId, now);
}
