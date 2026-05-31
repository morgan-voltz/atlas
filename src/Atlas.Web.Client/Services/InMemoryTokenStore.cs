namespace Atlas.Web.Client.Services;

/// <summary>
/// Stockage de l'access token en mémoire process (durée de vie du client WASM). Volontairement
/// non persisté : un rafraîchissement de l'onglet rejoue le refresh silencieux via le cookie HttpOnly.
/// </summary>
internal sealed class InMemoryTokenStore : ITokenStore
{
    private string? _accessToken;

    public string? GetAccessToken() => _accessToken;

    public void SetAccessToken(string token) => _accessToken = token;

    public void Clear() => _accessToken = null;
}
