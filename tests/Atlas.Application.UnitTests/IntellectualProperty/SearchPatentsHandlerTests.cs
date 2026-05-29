using Atlas.Application.IntellectualProperty.SearchPatents;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.IntellectualProperty;

public class SearchPatentsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _inpiCredentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly IIntellectualPropertyProvider _provider = Substitute.For<IIntellectualPropertyProvider>();

    public SearchPatentsHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) + "-dec");
    }

    private SearchPatentsHandler CreateHandler() => new(_inpiCredentials, _crypto, _provider);

    [Fact]
    public async Task Handle_returns_empty_search_when_all_criteria_blank()
    {
        Result<PagedResult<PatentSummaryDto>> result = await CreateHandler()
            .Handle(new SearchPatentsQuery(Guid.NewGuid(), null, "   ", "", 1, 20), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("patents.empty_search");
    }

    [Fact]
    public async Task Handle_returns_not_connected_when_no_inpi_credentials()
    {
        _inpiCredentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns((InpiCredentials?)null);

        Result<PagedResult<PatentSummaryDto>> result = await CreateHandler()
            .Handle(new SearchPatentsQuery(Guid.NewGuid(), "freinage", null, null, 1, 20), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task Handle_trims_criteria_before_forwarding_to_provider()
    {
        Guid userId = Guid.NewGuid();
        SetupUserWithInpi(userId);
        PatentSearchQuery? captured = null;
        _provider.SearchPatentsAsync(Arg.Do<PatentSearchQuery>(q => captured = q), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<PatentSummary>>.Ok(new PagedResult<PatentSummary>([], 1, 20, 0)));

        await CreateHandler().Handle(
            new SearchPatentsQuery(userId, "  freinage  ", "  Martin  ", null, 1, 20),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Title.Should().Be("freinage");
        captured.Inventor.Should().Be("Martin");
        captured.Applicant.Should().BeNull();
    }

    [Fact]
    public async Task Handle_clamps_invalid_pagination_to_defaults()
    {
        Guid userId = Guid.NewGuid();
        SetupUserWithInpi(userId);
        PatentSearchQuery? captured = null;
        _provider.SearchPatentsAsync(Arg.Do<PatentSearchQuery>(q => captured = q), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<PatentSummary>>.Ok(new PagedResult<PatentSummary>([], 1, 20, 0)));

        await CreateHandler().Handle(
            new SearchPatentsQuery(userId, "freinage", null, null, Page: 0, PageSize: 9999),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Page.Should().Be(1);
        captured.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_maps_provider_response_to_paged_dto()
    {
        Guid userId = Guid.NewGuid();
        SetupUserWithInpi(userId);

        var providerPage = new PagedResult<PatentSummary>(
            new[]
            {
                new PatentSummary(
                    PublicationNumber.FromTrustedValue("FR3045678B1"),
                    "Système de freinage",
                    "ACME SA",
                    new DateOnly(2023, 1, 15),
                    "Délivré"),
            },
            1, 20, 1);
        _provider.SearchPatentsAsync(Arg.Any<PatentSearchQuery>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PagedResult<PatentSummary>>.Ok(providerPage));

        Result<PagedResult<PatentSummaryDto>> result = await CreateHandler()
            .Handle(new SearchPatentsQuery(userId, "freinage", null, null, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
        result.Value!.Items[0].PublicationNumber.Should().Be("FR3045678B1");
        result.Value!.Items[0].Title.Should().Be("Système de freinage");
        result.Value!.TotalCount.Should().Be(1);
    }

    private void SetupUserWithInpi(Guid userId)
    {
        _inpiCredentials.GetByUserIdAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(userId), "u", "p", Now));
    }
}
