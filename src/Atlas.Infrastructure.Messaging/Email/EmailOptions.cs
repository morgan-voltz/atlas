namespace Atlas.Infrastructure.Messaging.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>URL de base du lien de vérification d'email (le userId et le token sont ajoutés en query string).</summary>
    public string VerificationBaseUrl { get; set; } = "https://localhost:7201/auth/verify-email";
}
