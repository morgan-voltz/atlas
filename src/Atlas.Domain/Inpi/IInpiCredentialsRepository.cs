using Atlas.Domain.Users;

namespace Atlas.Domain.Inpi;

public interface IInpiCredentialsRepository
{
    Task<InpiCredentials?> GetByUserIdAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(InpiCredentials credentials, CancellationToken ct = default);

    void Update(InpiCredentials credentials);

    Task DeleteByUserAsync(UserId userId, CancellationToken ct = default);
}
