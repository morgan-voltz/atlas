namespace Atlas.Application.Notifications;

/// <summary>DTO d'affichage pour un device enregistré (F-020). Le token n'est jamais renvoyé.</summary>
public sealed record DeviceRegistrationDto(
    Guid Id,
    string Platform,
    string? Label,
    DateTimeOffset RegisteredAt,
    DateTimeOffset LastSeenAt);
