namespace Atlas.Domain.Users;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    void Update(RefreshToken token);
}
