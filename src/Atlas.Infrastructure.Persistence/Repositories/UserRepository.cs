using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AtlasDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, ct);

    public Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, ct);

    public Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken ct = default) =>
        dbContext.Users.AnyAsync(user => user.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await dbContext.Users.AddAsync(user, ct);

    public void Update(User user) => dbContext.Users.Update(user);

    public Task DeleteAsync(UserId id, CancellationToken ct = default) =>
        dbContext.Users.Where(user => user.Id == id).ExecuteDeleteAsync(ct);
}
