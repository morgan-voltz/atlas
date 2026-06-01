using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Infrastructure.Security.Crypto;
using Atlas.Infrastructure.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atlas.Infrastructure.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Garde-fous (audit Lot 2a) : hors Development, les clés DOIVENT être configurées (idéalement via KMS).
        // ValidateOnStart fait échouer le démarrage de l'application si la condition n'est pas remplie.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            // Secret par fichier (ADR-019) : si la clé inline est absente mais qu'un chemin est fourni,
            // charge le PEM depuis le fichier monté. S'exécute avant la validation au démarrage.
            .PostConfigure(opts =>
            {
                if (string.IsNullOrWhiteSpace(opts.PrivateKeyPem)
                    && !string.IsNullOrWhiteSpace(opts.PrivateKeyPemFile)
                    && File.Exists(opts.PrivateKeyPemFile))
                {
                    opts.PrivateKeyPem = File.ReadAllText(opts.PrivateKeyPemFile);
                }
            })
            .Validate(
                opts => environment.IsDevelopment() || !string.IsNullOrWhiteSpace(opts.PrivateKeyPem),
                "Jwt:PrivateKeyPem doit être configuré hors Development (clé RSA persistée, KMS recommandé).")
            .ValidateOnStart();

        services.AddOptions<CryptoOptions>()
            .Bind(configuration.GetSection(CryptoOptions.SectionName))
            .PostConfigure(opts =>
            {
                if (string.IsNullOrWhiteSpace(opts.KeyBase64)
                    && !string.IsNullOrWhiteSpace(opts.KeyBase64File)
                    && File.Exists(opts.KeyBase64File))
                {
                    opts.KeyBase64 = File.ReadAllText(opts.KeyBase64File).Trim();
                }
            })
            .Validate(
                opts => environment.IsDevelopment() || !string.IsNullOrWhiteSpace(opts.KeyBase64),
                "Crypto:KeyBase64 doit être configuré hors Development (clé AES-256-GCM, KMS recommandé).")
            .ValidateOnStart();

        services.AddSingleton<ISigningKeyProvider, RsaSigningKeyProvider>();
        services.AddSingleton<IJwtIssuer, JwtIssuer>();
        services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddSingleton<ITokenGenerator, SecureTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ITotpProvider, TotpProvider>();
        services.AddSingleton<ICryptoService, AesGcmCryptoService>();
        services.AddSingleton<ITwoFactorChallengeService, TwoFactorChallengeService>();

        return services;
    }
}
