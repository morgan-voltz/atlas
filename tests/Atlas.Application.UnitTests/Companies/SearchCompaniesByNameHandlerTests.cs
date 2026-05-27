using Atlas.Application.Companies;
using Atlas.Application.Companies.SearchCompaniesByName;
using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Companies;

public class SearchCompaniesByNameHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _credentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ICompanyDataProvider _provider = Substitute.For<ICompanyDataProvider>();

    public SearchCompaniesByNameHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns("decrypted");
    }

    [Fact]
    public async Task Handle_returns_mapped_paged_results()
    {
        GiveConnectedAccount();
        var page = new PagedResult<CompanySummary>(
            [
                new CompanySummary(Siren.Create("552032534").Value!, "RENAULT", "Boulogne-Billancourt", new Naf("2910Z", null)),
                new CompanySummary(Siren.Create("775665019").Value!, "RENAULT TRUCKS", "Saint-Priest", null),
            ],
            Page: 1,
            PageSize: 20,
            TotalCount: 2);
        _provider.SearchByNameAsync(Arg.Any<CompanySearchQuery>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<CompanySummary>>.Ok(page));

        Result<PagedResult<CompanySummaryDto>> result = await CreateHandler()
            .Handle(new SearchCompaniesByNameQuery(Guid.NewGuid(), "renault"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items[0].Siren.Should().Be("552032534");
        result.Value!.Items[0].NafCode.Should().Be("2910Z");
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_without_connected_inpi_account_fails()
    {
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<PagedResult<CompanySummaryDto>> result = await CreateHandler()
            .Handle(new SearchCompaniesByNameQuery(Guid.NewGuid(), "renault"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task Handle_propagates_provider_unavailable()
    {
        GiveConnectedAccount();
        _provider.SearchByNameAsync(Arg.Any<CompanySearchQuery>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<CompanySummary>>.Fail(InpiErrors.Unavailable));

        Result<PagedResult<CompanySummaryDto>> result = await CreateHandler()
            .Handle(new SearchCompaniesByNameQuery(Guid.NewGuid(), "renault"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.unavailable");
    }

    private SearchCompaniesByNameHandler CreateHandler() => new(_credentials, _crypto, _provider);

    private void GiveConnectedAccount() =>
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(Guid.NewGuid()), "enc-u", "enc-p", Now));
}
