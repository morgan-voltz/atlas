using Atlas.Domain.Users;

namespace Atlas.Domain.Notifications;

public interface IDeviceRegistrationRepository
{
    Task<DeviceRegistration?> GetByIdAsync(DeviceRegistrationId id, CancellationToken ct = default);

    /// <summary>Recherche un device par son token (pour faire un upsert si le client renvoie le même token).</summary>
    Task<DeviceRegistration?> GetByTokenAsync(string token, CancellationToken ct = default);

    Task<IReadOnlyList<DeviceRegistration>> GetByUserAsync(UserId userId, CancellationToken ct = default);

    Task AddAsync(DeviceRegistration registration, CancellationToken ct = default);

    Task RemoveAsync(DeviceRegistration registration, CancellationToken ct = default);
}
