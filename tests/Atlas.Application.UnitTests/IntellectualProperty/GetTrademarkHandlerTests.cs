using System.Text;
using Atlas.Application.IntellectualProperty;
using Atlas.Application.IntellectualProperty.GetTrademark;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.IntellectualProperty;

public class GetTrademarkHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _credentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly IIntellectualPropertyProvider _provider = Substitute.For<IIntellectualPropertyProvider>();

    public GetTrademarkHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns("decrypted");
        GiveConnectedAccount();
    }

    [Fact]
    public async Task GetTrademark_returns_mapped_detail()
    {
        var detail = new TrademarkDetail(
            "NIKE", "Nike Inc.", new DepositNumber("4001234"),
            new DateOnly(2018, 3, 15), new DateOnly(2018, 9, 1), "Enregistrée", "verbale", HasImage: true,
            [new NiceClassification(25, "Vêtements"), new NiceClassification(35, null)]);
        _provider.GetTrademarkAsync(Arg.Any<DepositNumber>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrademarkDetail>.Ok(detail));

        Result<TrademarkDetailDto> result = await new GetTrademarkHandler(_credentials, _crypto, _provider)
            .Handle(new GetTrademarkQuery(Guid.NewGuid(), "4001234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Denomination.Should().Be("NIKE");
        result.Value!.HasImage.Should().BeTrue();
        result.Value!.ClassesNice.Should().HaveCount(2);
        result.Value!.ClassesNice[0].Number.Should().Be(25);
    }

    [Fact]
    public async Task GetTrademark_propagates_not_found()
    {
        _provider.GetTrademarkAsync(Arg.Any<DepositNumber>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrademarkDetail>.Fail(TrademarkErrors.NotFound(new DepositNumber("0"))));

        Result<TrademarkDetailDto> result = await new GetTrademarkHandler(_credentials, _crypto, _provider)
            .Handle(new GetTrademarkQuery(Guid.NewGuid(), "0"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("trademarks.not_found");
    }

    [Fact]
    public async Task GetTrademarkImage_returns_bytes()
    {
        byte[] png = Encoding.UTF8.GetBytes("fake-png");
        _provider.GetTrademarkImageAsync(Arg.Any<DepositNumber>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<TrademarkImage>.Ok(new TrademarkImage(png, "image/png")));

        Result<TrademarkImageDto> result = await new GetTrademarkImageHandler(_credentials, _crypto, _provider)
            .Handle(new GetTrademarkImageQuery(Guid.NewGuid(), "4001234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ContentType.Should().Be("image/png");
        result.Value!.Content.Should().Equal(png);
    }

    [Fact]
    public async Task GetTrademark_without_connected_account_fails()
    {
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>()).Returns((InpiCredentials?)null);

        Result<TrademarkDetailDto> result = await new GetTrademarkHandler(_credentials, _crypto, _provider)
            .Handle(new GetTrademarkQuery(Guid.NewGuid(), "4001234"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    private void GiveConnectedAccount() =>
        _credentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(Guid.NewGuid()), "enc-u", "enc-p", Now));
}
