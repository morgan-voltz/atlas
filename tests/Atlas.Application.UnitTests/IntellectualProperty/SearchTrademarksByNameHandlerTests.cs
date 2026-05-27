using Atlas.Application.IntellectualProperty;
using Atlas.Application.IntellectualProperty.SearchTrademarksByName;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.IntellectualProperty;

public class SearchTrademarksByNameHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _credentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly IIntellectualPropertyProvider _provider = Substitute.For<IIntellectualPropertyProvider>();

    public SearchTrademarksByNameHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns("decrypted");
    }

    [Fact]
    public async Task Handle_returns_mapped_trademarks()
    {
        GiveConnectedAccount();
        var page = new PagedResult<TrademarkSummary>(
            [
                new TrademarkSummary("NIKE", "Nike Inc.", new DepositNumber("4001234"), new DateOnly(2018, 3, 15), "Enregistrée"),
                new TrademarkSummary("NIKE AIR", "Nike Inc.", new DepositNumber("4005678"), null, "Enregistrée"),
            ],
            Page: 1,
            PageSize: 20,
            TotalCount: 2);
        _provider.SearchTrademarksAsync(Arg.Any<TrademarkSearchQuery>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<TrademarkSummary>>.Ok(page));

        Result<PagedResult<TrademarkSummaryDto>> result = await CreateHandler()
            .Handle(new SearchTrademarksByNameQuery(Guid.NewGuid(), "nike"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value!.Items[0].Denomination.Should().Be("NIKE");
        result.Value!.Items[0].DepositNumber.Should().Be("4001234");
        result.Value!.Items[0].DateDepot.Should().Be(new DateOnly(2018, 3, 15));
    }

    [Fact]
    public async Task Handle_without_connected_inpi_account_fails()
    {
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<PagedResult<TrademarkSummaryDto>> result = await CreateHandler()
            .Handle(new SearchTrademarksByNameQuery(Guid.NewGuid(), "nike"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    private SearchTrademarksByNameHandler CreateHandler() => new(_credentials, _crypto, _provider);

    private void GiveConnectedAccount() =>
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(Guid.NewGuid()), "enc-u", "enc-p", Now));
}
