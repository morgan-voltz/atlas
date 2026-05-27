using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Konscious.Security.Cryptography;

namespace Atlas.Infrastructure.Security.Crypto;

/// <summary>
/// Hachage de mot de passe avec Argon2id (cf. docs/04-securite-rgpd.md §5.4.1 : 64 Mo, 3 itérations, parallélisme 4).
/// Format encodé : {memoryKb}.{iterations}.{parallelism}.{saltBase64}.{hashBase64}.
/// </summary>
internal sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int MemoryKb = 65536; // 64 Mo
    private const int Iterations = 3;
    private const int DegreeOfParallelism = 4;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public PasswordHash Hash(string plainTextPassword)
    {
        ArgumentNullException.ThrowIfNull(plainTextPassword);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = ComputeHash(plainTextPassword, salt, MemoryKb, Iterations, DegreeOfParallelism, HashSize);

        string encoded = string.Join(
            '.',
            MemoryKb.ToString(CultureInfo.InvariantCulture),
            Iterations.ToString(CultureInfo.InvariantCulture),
            DegreeOfParallelism.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));

        return PasswordHash.FromHash(encoded);
    }

    public bool Verify(string plainTextPassword, PasswordHash hash)
    {
        ArgumentNullException.ThrowIfNull(plainTextPassword);

        string[] parts = hash.Value.Split('.');
        if (parts.Length != 5
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int memoryKb)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iterations)
            || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parallelism))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expected = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actual = ComputeHash(plainTextPassword, salt, memoryKb, iterations, parallelism, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] ComputeHash(
        string password,
        byte[] salt,
        int memoryKb,
        int iterations,
        int parallelism,
        int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKb,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };

        return argon2.GetBytes(hashSize);
    }
}
