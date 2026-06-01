using System.Security.Cryptography;
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

        // Le déchiffrement peut échouer si la clé KMS a tourné ou si le blob est corrompu (audit Lot 2,
        // M9). On le transforme en erreur métier plutôt que de laisser une CryptographicException
        // remonter : sinon un seul utilisateur au blob illisible ferait échouer tout un job batch.
        // Aucune valeur sensible n'est exposée (le message d'exception est ignoré).
        string username;
        string password;
        try
        {
            username = cryptoService.Decrypt(credentials.EncryptedUsername);
            password = cryptoService.Decrypt(credentials.EncryptedPassword);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            return Result<InpiAccessCredentials>.Fail(InpiErrors.CredentialsUnreadable);
        }

        return Result<InpiAccessCredentials>.Ok(new InpiAccessCredentials(username, password));
    }
}
