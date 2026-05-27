using Microsoft.IdentityModel.Tokens;

namespace Atlas.Infrastructure.Security.Jwt;

/// <summary>
/// Expose la clé de signature et les paramètres d'émission/validation des JWT, partagés entre
/// l'émetteur (côté infrastructure) et le middleware de validation (côté API).
/// </summary>
public interface ISigningKeyProvider
{
    SecurityKey SigningKey { get; }

    string Algorithm { get; }

    string Issuer { get; }

    string Audience { get; }
}
