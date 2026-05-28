namespace Atlas.Domain.Notifications;

/// <summary>
/// Charge utile d'une notification poussée à un utilisateur (F-020). Les <see cref="Data"/>
/// sont des paires clé/valeur silencieuses utilisées par le client pour router la notification
/// (ex. <c>{ "siren": "552032534" }</c> pour ouvrir la fiche entreprise).
/// </summary>
public sealed record NotificationPayload(string Title, string Body, IReadOnlyDictionary<string, string> Data);
