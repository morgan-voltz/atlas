namespace Atlas.Domain.Security;

/// <summary>
/// Génération et vérification TOTP (RFC 6238) pour la double authentification (cf. docs/04 §5.4.2).
/// </summary>
public interface ITotpProvider
{
    /// <summary>Génère un secret partagé encodé en Base32.</summary>
    string GenerateSecret();

    /// <summary>Construit l'URI <c>otpauth://</c> de provisioning (à encoder en QR code côté client).</summary>
    string BuildProvisioningUri(string secret, string accountName, string issuer);

    /// <summary>Vérifie un code TOTP par rapport au secret Base32, avec tolérance de fenêtre temporelle.</summary>
    bool VerifyCode(string secret, string code);
}
