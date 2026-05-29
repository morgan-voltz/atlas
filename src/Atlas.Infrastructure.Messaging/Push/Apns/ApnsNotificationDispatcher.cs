using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Push.Apns;

/// <summary>
/// Pousse les notifications vers iOS et macOS via l'API APNs HTTP/2 (F-020).
/// Cleanup auto sur 410 (Unregistered) et 400 (BadDeviceToken) — les tokens morts sont
/// supprimés silencieusement. Échecs transitoires : loggés et passés.
/// </summary>
internal sealed class ApnsNotificationDispatcher(
    HttpClient httpClient,
    IApnsAccessTokenProvider tokenProvider,
    IDeviceRegistrationRepository devices,
    IUnitOfWork unitOfWork,
    IOptions<ApnsOptions> options,
    ILogger<ApnsNotificationDispatcher> logger) : IPlatformPushDispatcher
{
    private static readonly DevicePlatform[] SupportedPlatforms = [DevicePlatform.ApnsIos, DevicePlatform.MacOsApns];

    private readonly ApnsOptions _options = options.Value;

    public async Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        IReadOnlyList<DeviceRegistration> userDevices = await devices.GetByUserAsync(userId, ct);
        var apnsDevices = userDevices
            .Where(d => SupportedPlatforms.Contains(d.Platform))
            .ToList();

        if (apnsDevices.Count == 0)
        {
            return;
        }

        string providerToken = await tokenProvider.GetTokenAsync(ct);
        bool devicesRemoved = false;

        foreach (DeviceRegistration device in apnsDevices)
        {
            ct.ThrowIfCancellationRequested();

            using var request = BuildRequest(device, payload, providerToken);

            try
            {
                using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                {
                    continue;
                }

                if (await IsDeviceInvalidAsync(response, ct))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation(
                            "APNs : token mort pour device {DeviceId} ({Status}), suppression.",
                            device.Id.Value,
                            (int)response.StatusCode);
                    }
                    await devices.RemoveAsync(device, ct);
                    devicesRemoved = true;
                    continue;
                }

                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "APNs : envoi en échec ({Status}) pour device {DeviceId}.",
                        (int)response.StatusCode,
                        device.Id.Value);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "APNs : exception réseau pour device {DeviceId}.", device.Id.Value);
                }
            }
        }

        if (devicesRemoved)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private HttpRequestMessage BuildRequest(DeviceRegistration device, NotificationPayload payload, string providerToken)
    {
        // Body APNs : "aps" est l'objet réservé ; les data utilisateur sont mises à plat à la racine.
        var body = new Dictionary<string, object?>(payload.Data.Count + 1)
        {
            ["aps"] = new
            {
                alert = new { title = payload.Title, body = payload.Body },
                sound = "default",
            },
        };
        foreach (KeyValuePair<string, string> kvp in payload.Data)
        {
            // Préserver "aps" si jamais le caller s'amusait à le mettre dans Data.
            if (!string.Equals(kvp.Key, "aps", StringComparison.Ordinal))
            {
                body[kvp.Key] = kvp.Value;
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/3/device/{device.Token}")
        {
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
            Content = JsonContent.Create(body),
        };

        // APNs exige les headers en minuscules pour HTTP/2.
        request.Headers.TryAddWithoutValidation("authorization", $"bearer {providerToken}");
        if (!string.IsNullOrWhiteSpace(_options.BundleId))
        {
            request.Headers.TryAddWithoutValidation("apns-topic", _options.BundleId);
        }
        request.Headers.TryAddWithoutValidation("apns-push-type", "alert");
        request.Headers.TryAddWithoutValidation("apns-priority", "10");

        return request;
    }

    private static async Task<bool> IsDeviceInvalidAsync(HttpResponseMessage response, CancellationToken ct)
    {
        // 410 Gone = Unregistered : l'app a été désinstallée ou le token a expiré.
        if (response.StatusCode == HttpStatusCode.Gone)
        {
            return true;
        }

        // 400 BadDeviceToken : token jamais valide pour ce topic (mauvaise URL, mauvaise app).
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            return false;
        }

        try
        {
            string raw = await response.Content.ReadAsStringAsync(ct);
            // APNs renvoie { "reason": "BadDeviceToken" } en JSON.
            using JsonDocument doc = JsonDocument.Parse(raw);
            return doc.RootElement.TryGetProperty("reason", out JsonElement reason)
                && reason.GetString() is "BadDeviceToken" or "DeviceTokenNotForTopic";
        }
        catch
        {
            return false;
        }
    }
}
