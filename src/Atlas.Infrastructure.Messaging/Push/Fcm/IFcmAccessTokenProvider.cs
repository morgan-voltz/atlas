namespace Atlas.Infrastructure.Messaging.Push.Fcm;

/// <summary>
/// Fournit un access token OAuth2 court (1 h) signé par le service account Firebase (F-020).
/// L'implémentation gère le cache mémoire (refresh proactif ~5 min avant expiration) et l'échange
/// JWT-bearer auprès de <c>oauth2.googleapis.com</c>.
/// </summary>
internal interface IFcmAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
}
