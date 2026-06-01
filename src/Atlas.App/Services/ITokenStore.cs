namespace Atlas.App.Services;

/// <summary>
/// Stocke l'access token JWT côté client. Sur la tête WebAssembly l'implémentation est
/// <see cref="InMemoryTokenStore"/> — strictement en mémoire, JAMAIS en <c>localStorage</c>
/// (anti-XSS, ADR-029/ADR-010). Sur les têtes natives, l'équivalent sera le secure storage de la
/// plateforme (Keychain/Keystore/DPAPI). Le refresh token n'est jamais vu par ce code : il vit dans
/// le cookie HttpOnly <c>atlas_refresh</c> (WASM) ou le secure storage (natif).
/// </summary>
public interface ITokenStore
{
    string? GetAccessToken();

    void SetAccessToken(string accessToken);

    void Clear();
}
