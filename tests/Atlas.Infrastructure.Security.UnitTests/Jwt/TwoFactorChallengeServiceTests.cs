using Atlas.Domain.Common;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Security.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Atlas.Infrastructure.Security.UnitTests.Jwt;

public sealed class TwoFactorChallengeServiceTests : IDisposable
{
    private readonly RsaSigningKeyProvider _keyProvider =
        new(Options.Create(new JwtOptions { Issuer = "atlas", Audience = "atlas", AccessTokenMinutes = 15 }));

    public void Dispose() => _keyProvider.Dispose();

    private TwoFactorChallengeService CreateService() => new(_keyProvider);

    [Fact]
    public async Task IssueChallenge_then_Validate_returns_same_userId()
    {
        TwoFactorChallengeService service = CreateService();
        UserId userId = UserId.New();

        string token = service.IssueChallenge(userId);
        UserId? validated = await service.ValidateChallengeAsync(token);

        validated.Should().Be(userId);
    }

    [Fact]
    public async Task ValidateChallenge_with_invalid_token_returns_null()
    {
        TwoFactorChallengeService service = CreateService();

        UserId? validated = await service.ValidateChallengeAsync("not.a.valid.jwt");

        validated.Should().BeNull();
    }

    [Fact]
    public async Task ValidateChallenge_with_tampered_signature_returns_null()
    {
        TwoFactorChallengeService service = CreateService();
        string token = service.IssueChallenge(UserId.New());
        string[] parts = token.Split('.');
        parts[2] = parts[2].Length > 0 ? parts[2][..^1] + (parts[2][^1] == 'A' ? 'B' : 'A') : "X";
        string tampered = string.Join('.', parts);

        UserId? validated = await service.ValidateChallengeAsync(tampered);

        validated.Should().BeNull();
    }

    [Fact]
    public async Task Access_token_issued_by_JwtIssuer_does_NOT_validate_as_2FA_challenge()
    {
        // Séparation d'audience (« atlas » vs « atlas-2fa ») : un access token ne doit jamais servir
        // de défi 2FA, et inversement. Garantie de la sécurité du flow 2FA.
        TwoFactorChallengeService challenge = CreateService();

        IDateTimeProvider clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero));
        JwtIssuer accessIssuer = new(
            _keyProvider,
            Options.Create(new JwtOptions { Issuer = "atlas", Audience = "atlas", AccessTokenMinutes = 15 }),
            clock);
        Atlas.Domain.Security.AccessToken accessToken = accessIssuer.Issue(
            UserId.New(),
            EmailAddress.Create($"u-{Guid.NewGuid():N}@example.com").Value!);

        UserId? validated = await challenge.ValidateChallengeAsync(accessToken.Value);

        validated.Should().BeNull();
    }
}
