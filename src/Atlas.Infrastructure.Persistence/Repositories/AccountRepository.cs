using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(AtlasDbContext dbContext) : IAccountRepository
{
    public async Task AddAsync(Account account, CancellationToken ct = default) =>
        await dbContext.Accounts.AddAsync(account, ct);

    public Task<Account?> GetByUserIdAsync(UserId userId, CancellationToken ct = default) =>
        dbContext.Accounts.FirstOrDefaultAsync(account => account.UserId == userId, ct);
}
