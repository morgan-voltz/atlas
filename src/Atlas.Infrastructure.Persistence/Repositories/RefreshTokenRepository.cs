using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(AtlasDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await dbContext.RefreshTokens.AddAsync(token, ct);

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

    public void Update(RefreshToken token) => dbContext.RefreshTokens.Update(token);

    public async Task RevokeAllForUserAsync(UserId userId, DateTimeOffset now, CancellationToken ct = default)
    {
        List<RefreshToken> active = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(ct);

        foreach (RefreshToken token in active)
        {
            token.Revoke(now);
        }
    }
}
