namespace Atlas.Domain.Users;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    void Update(RefreshToken token);

    /// <summary>Révoque toutes les sessions actives d'un utilisateur (ex. après réinitialisation de mot de passe).</summary>
    Task RevokeAllForUserAsync(UserId userId, DateTimeOffset now, CancellationToken ct = default);
}
