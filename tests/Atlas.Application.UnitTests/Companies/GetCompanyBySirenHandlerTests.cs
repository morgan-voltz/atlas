using Atlas.Application.Companies;
using Atlas.Application.Companies.GetCompanyBySiren;
using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Companies;

public class GetCompanyBySirenHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _credentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ICompanyDataProvider _provider = Substitute.For<ICompanyDataProvider>();

    public GetCompanyBySirenHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns("decrypted");
    }

    [Fact]
    public async Task Handle_with_valid_siren_and_connected_account_returns_mapped_company()
    {
        GiveConnectedAccount();
        _provider.GetBySirenAsync(Arg.Any<Siren>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Ok(SampleCompany()));

        Result<CompanyDto> result = await CreateHandler()
            .Handle(new GetCompanyBySirenQuery(Guid.NewGuid(), "552032534"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Siren.Should().Be("552032534");
        result.Value!.Denomination.Should().Be("RENAULT");
        result.Value!.NafCode.Should().Be("2910Z");
        result.Value!.Dirigeants.Should().ContainSingle(d => d.Nom == "Jean Dupont");
    }

    [Fact]
    public async Task Handle_with_invalid_siren_fails_without_calling_provider()
    {
        GiveConnectedAccount();

        Result<CompanyDto> result = await CreateHandler()
            .Handle(new GetCompanyBySirenQuery(Guid.NewGuid(), "not-a-siren"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.invalid_siren");
        await _provider.DidNotReceive().GetBySirenAsync(
            Arg.Any<Siren>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_without_connected_inpi_account_fails()
    {
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<CompanyDto> result = await CreateHandler()
            .Handle(new GetCompanyBySirenQuery(Guid.NewGuid(), "552032534"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task Handle_when_company_not_found_propagates_error()
    {
        GiveConnectedAccount();
        Siren siren = Siren.Create("552032534").Value!;
        _provider.GetBySirenAsync(Arg.Any<Siren>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<UniteLegale>.Fail(CompanyErrors.NotFound(siren)));

        Result<CompanyDto> result = await CreateHandler()
            .Handle(new GetCompanyBySirenQuery(Guid.NewGuid(), "552032534"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.not_found");
    }

    private GetCompanyBySirenHandler CreateHandler() => new(_credentials, _crypto, _provider);

    private void GiveConnectedAccount() =>
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(Guid.NewGuid()), "enc-u", "enc-p", Now));

    private static UniteLegale SampleCompany() =>
        new(
            Siren.Create("552032534").Value!,
            "RENAULT",
            "SA",
            new Naf("2910Z", "Construction de véhicules automobiles"),
            new Address("13 quai Le Gallo", "92100", "Boulogne-Billancourt", "France"),
            new DateOnly(1990, 1, 1),
            IsDiffusible: true,
            [new Dirigeant("Jean Dupont", "Président")]);
}
