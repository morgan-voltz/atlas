namespace Atlas.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);

    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    void Update(User user);
}
