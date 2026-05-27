namespace Atlas.Domain.Users;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken ct = default);

    Task<Account?> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
}
