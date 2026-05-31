namespace Atlas.Web.Client.Services;

/// <summary>
/// Stocke l'access token JWT côté client. Côté MAUI l'équivalent est le SecureStorage natif ;
/// côté web l'implémentation est <see cref="InMemoryTokenStore"/> — strictement en mémoire,
/// JAMAIS en <c>localStorage</c>/<c>sessionStorage</c> (anti-XSS, ADR-017). Le refresh token, lui,
/// n'est jamais vu par ce code : il vit dans le cookie HttpOnly <c>atlas_refresh</c> géré par le navigateur.
/// </summary>
public interface ITokenStore
{
    string? GetAccessToken();

    void SetAccessToken(string accessToken);

    void Clear();
}
