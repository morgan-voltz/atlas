using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Push.Fcm;

/// <summary>
/// Pousse les notifications vers Android et Web Push via l'API FCM HTTP v1 (F-020).
/// Échec « token invalide » (404 / UNREGISTERED) → suppression silencieuse du
/// <see cref="DeviceRegistration"/> concerné. Échecs transitoires : loggés et passés.
/// </summary>
internal sealed class FcmNotificationDispatcher(
    HttpClient httpClient,
    IFcmAccessTokenProvider tokenProvider,
    IDeviceRegistrationRepository devices,
    IUnitOfWork unitOfWork,
    IOptions<FcmOptions> options,
    ILogger<FcmNotificationDispatcher> logger) : INotificationDispatcher
{
    private readonly FcmOptions _options = options.Value;

    public async Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        IReadOnlyList<DeviceRegistration> userDevices = await devices.GetByUserAsync(userId, ct);
        var fcmDevices = userDevices
            .Where(d => d.Platform == DevicePlatform.FcmAndroid)
            .ToList();

        if (fcmDevices.Count == 0)
        {
            return;
        }

        string accessToken = await tokenProvider.GetAccessTokenAsync(ct);
        string url = $"/v1/projects/{_options.ProjectId}/messages:send";
        bool devicesRemoved = false;

        foreach (DeviceRegistration device in fcmDevices)
        {
            ct.ThrowIfCancellationRequested();

            var body = new
            {
                message = new
                {
                    token = device.Token,
                    notification = new { title = payload.Title, body = payload.Body },
                    data = payload.Data,
                },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                {
                    continue;
                }

                // 404 (NOT_FOUND) ou 400 avec errorCode UNREGISTERED → token mort, on supprime.
                if (await IsTokenInvalidAsync(response, ct))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation(
                            "FCM : token expiré pour device {DeviceId}, suppression.",
                            device.Id.Value);
                    }
                    await devices.RemoveAsync(device, ct);
                    devicesRemoved = true;
                    continue;
                }

                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "FCM : envoi en échec ({Status}) pour device {DeviceId}.",
                        (int)response.StatusCode,
                        device.Id.Value);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "FCM : exception réseau pour device {DeviceId}.", device.Id.Value);
                }
            }
        }

        if (devicesRemoved)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private static async Task<bool> IsTokenInvalidAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return true;
        }

        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            return false;
        }

        // FCM 400 avec error.details[*].errorCode = UNREGISTERED → token mort.
        try
        {
            string raw = await response.Content.ReadAsStringAsync(ct);
            return raw.Contains("UNREGISTERED", StringComparison.Ordinal)
                || raw.Contains("INVALID_ARGUMENT", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}
