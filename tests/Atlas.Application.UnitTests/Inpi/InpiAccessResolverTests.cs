using System.Security.Cryptography;
using Atlas.Application.Inpi;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Inpi;

public class InpiAccessResolverTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserGuid = Guid.NewGuid();

    private readonly IInpiCredentialsRepository _repository = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();

    [Fact]
    public async Task ResolveAsync_returns_not_connected_when_no_credentials()
    {
        _repository.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<InpiAccessCredentials> result =
            await InpiAccessResolver.ResolveAsync(_repository, _crypto, UserGuid, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task ResolveAsync_decrypts_credentials_on_happy_path()
    {
        StubStoredCredentials();
        _crypto.Decrypt("enc-user").Returns("user@inpi.fr");
        _crypto.Decrypt("enc-pass").Returns("s3cret");

        Result<InpiAccessCredentials> result =
            await InpiAccessResolver.ResolveAsync(_repository, _crypto, UserGuid, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Username.Should().Be("user@inpi.fr");
        result.Value!.Password.Should().Be("s3cret");
    }

    [Fact]
    public async Task ResolveAsync_returns_credentials_unreadable_when_decryption_fails()
    {
        // Audit Lot 2 (M9) : clé KMS tournée / blob corrompu => erreur métier, pas d'exception non gérée
        // qui ferait échouer tout un job batch.
        StubStoredCredentials();
        _crypto.Decrypt(Arg.Any<string>()).Returns(_ => throw new CryptographicException("tag mismatch"));

        Result<InpiAccessCredentials> result =
            await InpiAccessResolver.ResolveAsync(_repository, _crypto, UserGuid, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.credentials_unreadable");
    }

    private void StubStoredCredentials()
    {
        InpiCredentials stored = InpiCredentials.Create(new UserId(UserGuid), "enc-user", "enc-pass", Now);
        _repository.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns(stored);
    }
}
