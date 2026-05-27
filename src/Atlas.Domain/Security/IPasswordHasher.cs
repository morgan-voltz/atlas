using Atlas.Domain.Users;

namespace Atlas.Domain.Security;

/// <summary>
/// Port de hachage de mot de passe. Implémentation Argon2id (cf. docs/04-securite-rgpd.md §5.4.1).
/// </summary>
public interface IPasswordHasher
{
    PasswordHash Hash(string plainTextPassword);

    bool Verify(string plainTextPassword, PasswordHash hash);
}
