namespace Atlas.Domain.Notifications;

/// <summary>
/// Plateforme cible pour les notifications push (F-020). Les implémentations concrètes des dispatchers
/// (FCM, APNs, WNS, …) sont livrées en PRs séparées par plateforme.
/// </summary>
public enum DevicePlatform
{
    /// <summary>Android / Web via Firebase Cloud Messaging.</summary>
    FcmAndroid = 1,

    /// <summary>iOS via Apple Push Notification service.</summary>
    ApnsIos = 2,

    /// <summary>Windows desktop via Windows Notification Service (UWP/WinUI).</summary>
    WindowsWns = 3,

    /// <summary>macOS desktop via Apple Push Notification service.</summary>
    MacOsApns = 4,
}
