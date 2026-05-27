using Microsoft.Maui.Storage;

namespace Atlas.Maui.Services;

internal sealed class SecureStorageTokenStore : ITokenStore
{
    private const string AccessTokenKey = "atlas.access_token";

    private readonly ISecureStorage _secureStorage;

    public SecureStorageTokenStore(ISecureStorage secureStorage) => _secureStorage = secureStorage;

    public Task<string?> GetAccessTokenAsync() => _secureStorage.GetAsync(AccessTokenKey);

    public Task SetAccessTokenAsync(string accessToken) => _secureStorage.SetAsync(AccessTokenKey, accessToken);

    public void Clear() => _secureStorage.Remove(AccessTokenKey);
}
