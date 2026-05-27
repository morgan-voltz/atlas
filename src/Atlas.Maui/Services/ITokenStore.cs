namespace Atlas.Maui.Services;

/// <summary>
/// Stockage sécurisé du jeton de session (Keychain iOS / Keystore Android via SecureStorage).
/// Aucun credential INPI n'est stocké sur le device (cf. ADR-002, F-009).
/// </summary>
public interface ITokenStore
{
    Task<string?> GetAccessTokenAsync();

    Task SetAccessTokenAsync(string accessToken);

    void Clear();
}
