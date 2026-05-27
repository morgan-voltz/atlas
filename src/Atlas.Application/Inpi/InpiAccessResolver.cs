using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;

namespace Atlas.Application.Inpi;

/// <summary>
/// Charge les identifiants INPI d'un utilisateur et les déchiffre en <see cref="InpiAccessCredentials"/>
/// (en clair, transient). Retourne <see cref="InpiErrors.NotConnected"/> si aucun compte n'est connecté.
/// </summary>
internal static class InpiAccessResolver
{
    public static async Task<Result<InpiAccessCredentials>> ResolveAsync(
        IInpiCredentialsRepository repository,
        ICryptoService cryptoService,
        Guid userId,
        CancellationToken ct)
    {
        InpiCredentials? credentials = await repository.GetByUserIdAsync(new UserId(userId), ct);
        if (credentials is null)
        {
            return Result<InpiAccessCredentials>.Fail(InpiErrors.NotConnected);
        }

        var access = new InpiAccessCredentials(
            cryptoService.Decrypt(credentials.EncryptedUsername),
            cryptoService.Decrypt(credentials.EncryptedPassword));

        return Result<InpiAccessCredentials>.Ok(access);
    }
}
