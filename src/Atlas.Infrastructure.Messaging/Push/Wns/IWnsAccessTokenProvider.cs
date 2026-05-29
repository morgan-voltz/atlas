namespace Atlas.Infrastructure.Messaging.Push.Wns;

/// <summary>
/// Fournit un access token WNS court (TTL ~24 h) obtenu par OAuth2 <c>client_credentials</c>
/// sur <c>login.live.com</c>. Cache thread-safe avec refresh proactif.
/// </summary>
internal interface IWnsAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
}
