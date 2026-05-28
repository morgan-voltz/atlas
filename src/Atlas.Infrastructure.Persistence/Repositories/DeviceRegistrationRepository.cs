using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Repositories;

internal sealed class DeviceRegistrationRepository(AtlasDbContext dbContext) : IDeviceRegistrationRepository
{
    public Task<DeviceRegistration?> GetByIdAsync(DeviceRegistrationId id, CancellationToken ct = default) =>
        dbContext.DeviceRegistrations.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<DeviceRegistration?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        dbContext.DeviceRegistrations.FirstOrDefaultAsync(d => d.Token == token, ct);

    public async Task<IReadOnlyList<DeviceRegistration>> GetByUserAsync(UserId userId, CancellationToken ct = default) =>
        await dbContext.DeviceRegistrations
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(ct);

    public async Task AddAsync(DeviceRegistration registration, CancellationToken ct = default) =>
        await dbContext.DeviceRegistrations.AddAsync(registration, ct);

    public Task RemoveAsync(DeviceRegistration registration, CancellationToken ct = default)
    {
        dbContext.DeviceRegistrations.Remove(registration);
        return Task.CompletedTask;
    }
}
