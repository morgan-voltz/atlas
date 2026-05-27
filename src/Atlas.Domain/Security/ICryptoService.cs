namespace Atlas.Domain.Security;

/// <summary>
/// Chiffrement/déchiffrement symétrique au repos (AES-256-GCM, clé gérée via KMS — cf. docs/04 et docs/09 §5.3).
/// Utilisé pour les secrets TOTP, et à terme les credentials INPI (F-003).
/// </summary>
public interface ICryptoService
{
    string Encrypt(string plaintext);

    string Decrypt(string ciphertext);
}
