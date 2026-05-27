namespace Atlas.Domain.Security;

/// <summary>
/// Génère des jetons opaques à haute entropie (vérification d'email, refresh tokens) et leur hash de stockage.
/// Le clair est transmis à l'utilisateur ; seul le hash est persisté.
/// </summary>
public interface ITokenGenerator
{
    string GenerateUrlSafeToken(int byteLength = 32);

    string Hash(string token);
}
