using Atlas.Application.IntellectualProperty.GetPatent;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.IntellectualProperty;

public class GetPatentHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _inpiCredentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly IIntellectualPropertyProvider _provider = Substitute.For<IIntellectualPropertyProvider>();

    public GetPatentHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) + "-dec");
    }

    private GetPatentHandler CreateHandler() => new(_inpiCredentials, _crypto, _provider);

    [Fact]
    public async Task Handle_returns_invalid_publication_number_when_too_short()
    {
        Result<PatentDetailDto> result = await CreateHandler()
            .Handle(new GetPatentQuery(Guid.NewGuid(), "FR"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("patents.invalid_publication_number");
    }

    [Fact]
    public async Task Handle_returns_invalid_publication_number_on_disallowed_char()
    {
        Result<PatentDetailDto> result = await CreateHandler()
            .Handle(new GetPatentQuery(Guid.NewGuid(), "FR123 ABC!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("patents.invalid_publication_number");
    }

    [Fact]
    public async Task Handle_returns_not_connected_when_no_inpi_credentials()
    {
        _inpiCredentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns((InpiCredentials?)null);

        Result<PatentDetailDto> result = await CreateHandler()
            .Handle(new GetPatentQuery(Guid.NewGuid(), "FR3045678B1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task Handle_normalizes_publication_number_to_uppercase_without_spaces()
    {
        Guid userId = Guid.NewGuid();
        _inpiCredentials.GetByUserIdAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(userId), "u", "p", Now));

        PublicationNumber? captured = null;
        _provider.GetPatentByPublicationNumberAsync(
                Arg.Do<PublicationNumber>(p => captured = p),
                Arg.Any<InpiAccessCredentials>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<PatentDetail>.Ok(NewSample()));

        await CreateHandler()
            .Handle(new GetPatentQuery(userId, "fr 30 45 678 b1"), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Value.Value.Should().Be("FR3045678B1");
    }

    [Fact]
    public async Task Handle_maps_provider_response_to_dto()
    {
        Guid userId = Guid.NewGuid();
        _inpiCredentials.GetByUserIdAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(userId), "u", "p", Now));
        _provider.GetPatentByPublicationNumberAsync(Arg.Any<PublicationNumber>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<PatentDetail>.Ok(NewSample()));

        Result<PatentDetailDto> result = await CreateHandler()
            .Handle(new GetPatentQuery(userId, "FR3045678B1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PublicationNumber.Should().Be("FR3045678B1");
        result.Value!.Title.Should().Be("Système de freinage");
        result.Value!.Applicant.Should().Be("ACME SA");
        result.Value!.Inventors.Should().BeEquivalentTo("Alice Martin", "Bob Durand");
        result.Value!.DepositDate.Should().Be(new DateOnly(2023, 1, 15));
    }

    private static PatentDetail NewSample() => new(
        PublicationNumber.FromTrustedValue("FR3045678B1"),
        "Système de freinage",
        "ACME SA",
        ["Alice Martin", "Bob Durand"],
        new DateOnly(2023, 1, 15),
        new DateOnly(2024, 7, 10),
        "Délivré",
        "Brevet portant sur un système de freinage régénératif.");
}
