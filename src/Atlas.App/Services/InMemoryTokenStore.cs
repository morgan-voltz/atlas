namespace Atlas.App.Services;

/// <summary>
/// Stockage de l'access token en mémoire process. Volontairement non persisté : un redémarrage
/// rejoue le refresh silencieux via le cookie HttpOnly (WASM) / secure storage (natif).
/// </summary>
internal sealed class InMemoryTokenStore : ITokenStore
{
    private string? _accessToken;

    public string? GetAccessToken() => _accessToken;

    public void SetAccessToken(string accessToken) => _accessToken = accessToken;

    public void Clear() => _accessToken = null;
}
