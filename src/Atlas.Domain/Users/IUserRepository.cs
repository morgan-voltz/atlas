namespace Atlas.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);

    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    void Update(User user);

    /// <summary>Supprime l'utilisateur et, par cascade, toutes ses données liées (droit à l'effacement RGPD).</summary>
    Task DeleteAsync(UserId id, CancellationToken ct = default);
}
