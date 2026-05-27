using Atlas.Domain.Common;
using Atlas.Domain.Security;
using Atlas.Infrastructure.Security.Crypto;
using Atlas.Infrastructure.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CryptoOptions>(configuration.GetSection(CryptoOptions.SectionName));

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
