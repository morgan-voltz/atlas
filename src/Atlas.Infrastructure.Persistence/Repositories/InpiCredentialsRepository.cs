using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class InpiCredentialsRepository(AtlasDbContext dbContext) : IInpiCredentialsRepository
{
    public Task<InpiCredentials?> GetByUserIdAsync(UserId userId, CancellationToken ct = default) =>
        dbContext.InpiCredentials.FirstOrDefaultAsync(credentials => credentials.UserId == userId, ct);

    public async Task AddAsync(InpiCredentials credentials, CancellationToken ct = default) =>
        await dbContext.InpiCredentials.AddAsync(credentials, ct);

    public void Update(InpiCredentials credentials) => dbContext.InpiCredentials.Update(credentials);

    public Task DeleteByUserAsync(UserId userId, CancellationToken ct = default) =>
        dbContext.InpiCredentials
            .Where(credentials => credentials.UserId == userId)
            .ExecuteDeleteAsync(ct);
}
