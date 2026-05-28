using Atlas.Domain.Common;
using Atlas.Domain.Users;

namespace Atlas.Domain.Notifications;

/// <summary>
/// Token de notification push enregistré pour un device d'un utilisateur (F-020).
/// Un user peut avoir N devices (téléphone Android + iPhone + PC portable + macOS).
/// Le token est fourni par la plateforme (FCM/APNs/WNS) et change parfois — il est mis
/// à jour par un POST /devices (upsert sur le token).
/// </summary>
public sealed class DeviceRegistration : Entity<DeviceRegistrationId>
{
    public const int MaxTokenLength = 4096;
    public const int MaxLabelLength = 128;

    private DeviceRegistration()
        : base(default)
    {
        // Réhydratation EF Core.
    }

    private DeviceRegistration(
        DeviceRegistrationId id,
        UserId userId,
        DevicePlatform platform,
        string token,
        string? label,
        DateTimeOffset registeredAt)
        : base(id)
    {
        UserId = userId;
        Platform = platform;
        Token = token;
        Label = label;
        RegisteredAt = registeredAt;
        LastSeenAt = registeredAt;
    }

    public UserId UserId { get; private set; }

    public DevicePlatform Platform { get; private set; }

    public string Token { get; private set; } = null!;

    /// <summary>Étiquette lisible facultative (« iPhone de Morgan »).</summary>
    public string? Label { get; private set; }

    public DateTimeOffset RegisteredAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public static DeviceRegistration Register(
        UserId userId,
        DevicePlatform platform,
        string token,
        string? label,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        string normalizedToken = token.Trim();
        if (normalizedToken.Length > MaxTokenLength)
        {
            throw new ArgumentException($"Token push trop long (>{MaxTokenLength} caractères).", nameof(token));
        }

        string? normalizedLabel = label?.Trim();
        if (!string.IsNullOrEmpty(normalizedLabel) && normalizedLabel.Length > MaxLabelLength)
        {
            normalizedLabel = normalizedLabel[..MaxLabelLength];
        }

        return new DeviceRegistration(
            DeviceRegistrationId.New(),
            userId,
            platform,
            normalizedToken,
            normalizedLabel,
            now);
    }

    /// <summary>Marque le device comme actif (le client a poussé un nouveau token / a rafraîchi).</summary>
    public void Touch(DateTimeOffset now) => LastSeenAt = now;
}
