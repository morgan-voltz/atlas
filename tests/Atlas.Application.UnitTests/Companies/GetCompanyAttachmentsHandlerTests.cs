using Atlas.Application.Companies.GetCompanyAttachments;
using Atlas.Domain.Companies;
using Atlas.Domain.Companies.Attachments;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Companies;

public class GetCompanyAttachmentsHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _inpiCredentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ICompanyDataProvider _companyProvider = Substitute.For<ICompanyDataProvider>();

    public GetCompanyAttachmentsHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) + "-dec");
    }

    private GetCompanyAttachmentsHandler CreateHandler() =>
        new(_inpiCredentials, _crypto, _companyProvider);

    [Fact]
    public async Task Handle_returns_invalid_siren_when_format_wrong()
    {
        Result<IReadOnlyList<CompanyAttachmentDto>> result = await CreateHandler()
            .Handle(new GetCompanyAttachmentsQuery(Guid.NewGuid(), "ABC"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.invalid_siren");
    }

    [Fact]
    public async Task Handle_returns_not_connected_when_no_inpi_credentials()
    {
        _inpiCredentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns((InpiCredentials?)null);

        Result<IReadOnlyList<CompanyAttachmentDto>> result = await CreateHandler()
            .Handle(new GetCompanyAttachmentsQuery(Guid.NewGuid(), "552032534"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task Handle_maps_attachments_to_dto()
    {
        Guid userId = Guid.NewGuid();
        _inpiCredentials.GetByUserIdAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(userId), "u", "p", Now));

        _companyProvider.GetAttachmentsAsync(Arg.Any<Siren>(), Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<CompanyAttachment>>.Ok(new List<CompanyAttachment>
            {
                new("acte-1", AttachmentType.Acte, "Statuts à jour", new DateOnly(2024, 1, 15), 12345, false),
                new("bilan-2024", AttachmentType.Bilan, "Comptes 2024", new DateOnly(2025, 6, 30), 67890, true),
            }));

        Result<IReadOnlyList<CompanyAttachmentDto>> result = await CreateHandler()
            .Handle(new GetCompanyAttachmentsQuery(userId, "552032534"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value![0].Type.Should().Be("Acte");
        result.Value![0].IsConfidential.Should().BeFalse();
        result.Value![1].Type.Should().Be("Bilan");
        result.Value![1].IsConfidential.Should().BeTrue();
    }
}
