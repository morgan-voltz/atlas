using Atlas.Application.Companies.DownloadCompanyAttachment;
using Atlas.Domain.Companies;
using Atlas.Domain.Companies.Attachments;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Companies;

public class DownloadCompanyAttachmentHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly IInpiCredentialsRepository _inpiCredentials = Substitute.For<IInpiCredentialsRepository>();
    private readonly ICryptoService _crypto = Substitute.For<ICryptoService>();
    private readonly ICompanyDataProvider _companyProvider = Substitute.For<ICompanyDataProvider>();

    public DownloadCompanyAttachmentHandlerTests()
    {
        _crypto.Decrypt(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) + "-dec");
    }

    private DownloadCompanyAttachmentHandler CreateHandler() =>
        new(_inpiCredentials, _crypto, _companyProvider);

    [Fact]
    public async Task Handle_returns_invalid_siren_when_format_wrong()
    {
        Result<AttachmentContent> result = await CreateHandler()
            .Handle(new DownloadCompanyAttachmentQuery(Guid.NewGuid(), "ABC", "doc-1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.invalid_siren");
    }

    [Fact]
    public async Task Handle_returns_attachment_not_found_when_attachmentId_empty()
    {
        Result<AttachmentContent> result = await CreateHandler()
            .Handle(new DownloadCompanyAttachmentQuery(Guid.NewGuid(), "552032534", ""), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("companies.attachment_not_found");
    }

    [Fact]
    public async Task Handle_returns_not_connected_when_no_inpi_credentials()
    {
        _inpiCredentials.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns((InpiCredentials?)null);

        Result<AttachmentContent> result = await CreateHandler()
            .Handle(new DownloadCompanyAttachmentQuery(Guid.NewGuid(), "552032534", "doc-1"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("inpi.not_connected");
    }

    [Fact]
    public async Task Handle_returns_content_when_provider_succeeds()
    {
        Guid userId = Guid.NewGuid();
        _inpiCredentials.GetByUserIdAsync(new UserId(userId), Arg.Any<CancellationToken>())
            .Returns(InpiCredentials.Create(new UserId(userId), "u", "p", Now));

        var contentStream = new MemoryStream([0x25, 0x50, 0x44, 0x46]); // "%PDF"
        _companyProvider.DownloadAttachmentAsync(
                Arg.Any<Siren>(), "doc-1", Arg.Any<InpiAccessCredentials>(), Arg.Any<CancellationToken>())
            .Returns(Result<AttachmentContent>.Ok(new AttachmentContent(contentStream, "application/pdf", "552032534_doc-1.pdf")));

        Result<AttachmentContent> result = await CreateHandler()
            .Handle(new DownloadCompanyAttachmentQuery(userId, "552032534", "doc-1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ContentType.Should().Be("application/pdf");
        result.Value!.FileName.Should().Be("552032534_doc-1.pdf");
    }
}
