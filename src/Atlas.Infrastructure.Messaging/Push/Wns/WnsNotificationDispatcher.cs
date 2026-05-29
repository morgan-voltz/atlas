using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml;
using Atlas.Domain.Common;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Atlas.Infrastructure.Messaging.Push.Wns;

/// <summary>
/// Pousse les notifications vers Windows desktop (UWP / WinUI / MAUI-Windows) via WNS (F-020).
/// La cible HTTP est l'<c>ChannelUri</c> stockée dans <see cref="DeviceRegistration.Token"/>
/// — chaque device a son propre endpoint, pas de <c>BaseAddress</c> partagée.
/// Payload : XML toast ToastGeneric avec deux <c>text</c> (Title + Body). Données utilisateur
/// sérialisées en JSON dans l'attribut <c>launch</c> (récupéré côté app via <c>e.Argument</c>).
/// Cleanup auto sur 410 Gone et 404 NotFound — le ChannelUri est mort.
/// </summary>
internal sealed class WnsNotificationDispatcher(
    HttpClient httpClient,
    IWnsAccessTokenProvider tokenProvider,
    IDeviceRegistrationRepository devices,
    IUnitOfWork unitOfWork,
    ILogger<WnsNotificationDispatcher> logger) : IPlatformPushDispatcher
{
    public async Task DispatchAsync(UserId userId, NotificationPayload payload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        IReadOnlyList<DeviceRegistration> userDevices = await devices.GetByUserAsync(userId, ct);
        var wnsDevices = userDevices
            .Where(d => d.Platform == DevicePlatform.WindowsWns)
            .ToList();

        if (wnsDevices.Count == 0)
        {
            return;
        }

        string accessToken = await tokenProvider.GetAccessTokenAsync(ct);
        string toastXml = BuildToastXml(payload);
        bool devicesRemoved = false;

        foreach (DeviceRegistration device in wnsDevices)
        {
            ct.ThrowIfCancellationRequested();

            // L'URL du push est le ChannelUri stocké dans Token — URL absolue.
            if (!Uri.TryCreate(device.Token, UriKind.Absolute, out Uri? channelUri))
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "WNS : ChannelUri invalide pour device {DeviceId}, suppression.",
                        device.Id.Value);
                }
                await devices.RemoveAsync(device, ct);
                devicesRemoved = true;
                continue;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, channelUri)
            {
                Content = new StringContent(toastXml, Encoding.UTF8, "text/xml"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.TryAddWithoutValidation("X-WNS-Type", "wns/toast");
            request.Headers.TryAddWithoutValidation("X-WNS-RequestForStatus", "true");

            try
            {
                using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                {
                    continue;
                }

                if (IsChannelDead(response))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation(
                            "WNS : ChannelUri mort pour device {DeviceId} ({Status}), suppression.",
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
                        "WNS : envoi en échec ({Status}) pour device {DeviceId}.",
                        (int)response.StatusCode,
                        device.Id.Value);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "WNS : exception réseau pour device {DeviceId}.", device.Id.Value);
                }
            }
        }

        if (devicesRemoved)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Construit le XML toast au format ToastGeneric (template par défaut Windows 10/11).
    /// Les données utilisateur sont injectées en JSON dans l'attribut <c>launch</c> ;
    /// l'app peut les récupérer via <c>ToastNotificationActivatedEventArgs.Argument</c>.
    /// </summary>
    internal static string BuildToastXml(NotificationPayload payload)
    {
        string launchJson = payload.Data.Count == 0 ? string.Empty : JsonSerializer.Serialize(payload.Data);

        var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
        var sb = new StringBuilder();
        using (XmlWriter writer = XmlWriter.Create(sb, settings))
        {
            writer.WriteStartElement("toast");
            if (!string.IsNullOrEmpty(launchJson))
            {
                writer.WriteAttributeString("launch", launchJson);
            }

            writer.WriteStartElement("visual");
            writer.WriteStartElement("binding");
            writer.WriteAttributeString("template", "ToastGeneric");

            writer.WriteStartElement("text");
            writer.WriteString(payload.Title);
            writer.WriteEndElement(); // text

            writer.WriteStartElement("text");
            writer.WriteString(payload.Body);
            writer.WriteEndElement(); // text

            writer.WriteEndElement(); // binding
            writer.WriteEndElement(); // visual
            writer.WriteEndElement(); // toast
        }

        return sb.ToString();
    }

    /// <summary>
    /// WNS renvoie 410 Gone ou 404 NotFound quand le ChannelUri n'est plus valide
    /// (app désinstallée, channel expiré).
    /// </summary>
    private static bool IsChannelDead(HttpResponseMessage response) =>
        response.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound;
}
