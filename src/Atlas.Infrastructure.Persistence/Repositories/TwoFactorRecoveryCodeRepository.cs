using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class TwoFactorRecoveryCodeRepository(AtlasDbContext dbContext) : ITwoFactorRecoveryCodeRepository
{
    public async Task AddRangeAsync(IEnumerable<TwoFactorRecoveryCode> codes, CancellationToken ct = default) =>
        await dbContext.TwoFactorRecoveryCodes.AddRangeAsync(codes, ct);

    public Task<TwoFactorRecoveryCode?> GetActiveByHashAsync(
        UserId userId,
        string codeHash,
        CancellationToken ct = default) =>
        dbContext.TwoFactorRecoveryCodes.FirstOrDefaultAsync(
            code => code.UserId == userId && code.CodeHash == codeHash && code.UsedAt == null,
            ct);

    public Task DeleteByUserAsync(UserId userId, CancellationToken ct = default) =>
        dbContext.TwoFactorRecoveryCodes
            .Where(code => code.UserId == userId)
            .ExecuteDeleteAsync(ct);

    public void Update(TwoFactorRecoveryCode code) => dbContext.TwoFactorRecoveryCodes.Update(code);
}
