namespace Atlas.Domain.Users;

/// <summary>
/// Encapsule un hash de mot de passe déjà calculé (Argon2id). Ne contient jamais le mot de passe en clair.
/// </summary>
public readonly record struct PasswordHash
{
    private PasswordHash(string value) => Value = value;

    public string Value { get; }

    public static PasswordHash FromHash(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        return new PasswordHash(hash);
    }

    public override string ToString() => "***";
}
