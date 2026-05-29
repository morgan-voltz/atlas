using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class FeedRuleRepository(AtlasDbContext dbContext) : IFeedRuleRepository
{
    public async Task<FeedRule?> GetByIdAsync(FeedRuleId id, CancellationToken ct = default) =>
        await dbContext.FeedRules
            .FirstOrDefaultAsync(rule => rule.Id == id, ct);

    public async Task<IReadOnlyList<FeedRule>> ListByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.FeedRules
            .Where(rule => rule.UserId == userId)
            .OrderByDescending(rule => rule.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<FeedRule>> ListActiveAsync(CancellationToken ct = default) =>
        await dbContext.FeedRules
            .Where(rule => rule.IsActive)
            .ToListAsync(ct);

    public async Task AddAsync(FeedRule rule, CancellationToken ct = default) =>
        await dbContext.FeedRules.AddAsync(rule, ct);

    public Task RemoveAsync(FeedRule rule, CancellationToken ct = default)
    {
        dbContext.FeedRules.Remove(rule);
        return Task.CompletedTask;
    }
}
