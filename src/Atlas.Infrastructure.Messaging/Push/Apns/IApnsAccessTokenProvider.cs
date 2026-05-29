namespace Atlas.Infrastructure.Messaging.Push.Apns;

/// <summary>
/// Fournit un JWT signé ES256 (P-256 ECDSA, claim <c>iss</c> = Team ID) utilisable comme provider
/// authentication token APNs. Apple recommande un rafraîchissement entre 20 min et 1 h.
/// </summary>
internal interface IApnsAccessTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}
