using System.Collections.Concurrent;

namespace Atlas.Api.Dev;

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Mémorise en RAM le dernier token de vérification d'email émis par compte,
/// pour automatiser les tests (l'email réel n'étant pas envoyé en dev). Ce service n'est enregistré que
/// si l'environnement est Development : il n'existe pas en production.
/// </summary>
internal sealed class DevVerificationTokenStore
{
    private readonly ConcurrentDictionary<string, DevVerificationEntry> _byEmail =
        new(StringComparer.OrdinalIgnoreCase);

    public void Record(string email, Guid userId, string token) =>
        _byEmail[email] = new DevVerificationEntry(userId, token);

    public bool TryGet(string email, out DevVerificationEntry entry) =>
        _byEmail.TryGetValue(email, out entry!);
}

internal sealed record DevVerificationEntry(Guid UserId, string Token);
