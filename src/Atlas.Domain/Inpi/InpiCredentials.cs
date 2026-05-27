using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Inpi;

/// <summary>
/// Identifiants INPI d'un utilisateur (modèle multi-tenant, ADR-003), chiffrés au repos via
/// <see cref="Atlas.Domain.Security.ICryptoService"/>. Le clair n'existe qu'en mémoire le temps d'une requête
/// et n'est JAMAIS loggé ni exposé par l'API (cf. CLAUDE.md, docs/04).
/// </summary>
public sealed class InpiCredentials : Entity<InpiCredentialsId>
{
    private InpiCredentials()
        : base(default)
    {
        // Constructeur de réhydratation EF Core.
    }

    private InpiCredentials(
        InpiCredentialsId id,
        UserId userId,
        string encryptedUsername,
        string encryptedPassword,
        DateTimeOffset now)
        : base(id)
    {
        UserId = userId;
        EncryptedUsername = encryptedUsername;
        EncryptedPassword = encryptedPassword;
        Status = InpiCredentialsStatus.Active;
        CreatedAt = now;
        UpdatedAt = now;
        LastTestedAt = now;
    }

    public UserId UserId { get; private set; }

    public string EncryptedUsername { get; private set; } = null!;

    public string EncryptedPassword { get; private set; } = null!;

    public InpiCredentialsStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? LastTestedAt { get; private set; }

    /// <summary>Crée des credentials après un test de connexion réussi (statut <see cref="InpiCredentialsStatus.Active"/>).</summary>
    public static InpiCredentials Create(
        UserId userId,
        string encryptedUsername,
        string encryptedPassword,
        DateTimeOffset now) =>
        new(InpiCredentialsId.New(), userId, encryptedUsername, encryptedPassword, now);

    /// <summary>Remplace les identifiants (re-connexion) après un nouveau test réussi.</summary>
    public void UpdateCredentials(string encryptedUsername, string encryptedPassword, DateTimeOffset now)
    {
        EncryptedUsername = encryptedUsername;
        EncryptedPassword = encryptedPassword;
        Status = InpiCredentialsStatus.Active;
        UpdatedAt = now;
        LastTestedAt = now;
    }

    /// <summary>Met à jour le statut suite à un test de connectivité (ex. mot de passe INPI expiré).</summary>
    public void MarkTested(bool success, DateTimeOffset now)
    {
        Status = success ? InpiCredentialsStatus.Active : InpiCredentialsStatus.Invalid;
        LastTestedAt = now;
    }
}
