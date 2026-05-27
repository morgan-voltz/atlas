namespace Atlas.Domain.Users;

public interface ITwoFactorRecoveryCodeRepository
{
    Task AddRangeAsync(IEnumerable<TwoFactorRecoveryCode> codes, CancellationToken ct = default);

    Task<TwoFactorRecoveryCode?> GetActiveByHashAsync(UserId userId, string codeHash, CancellationToken ct = default);

    Task DeleteByUserAsync(UserId userId, CancellationToken ct = default);

    void Update(TwoFactorRecoveryCode code);
}
