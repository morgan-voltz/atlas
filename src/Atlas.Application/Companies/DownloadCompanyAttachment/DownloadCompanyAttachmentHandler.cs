using Atlas.Application.Inpi;
using Atlas.Domain.Companies;
using Atlas.Domain.Companies.Attachments;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.DownloadCompanyAttachment;

internal sealed class DownloadCompanyAttachmentHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    ICompanyDataProvider companyDataProvider)
    : IRequestHandler<DownloadCompanyAttachmentQuery, Result<AttachmentContent>>
{
    public async Task<Result<AttachmentContent>> Handle(
        DownloadCompanyAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        Result<Siren> sirenResult = Siren.Create(request.Siren);
        if (sirenResult.IsFailure)
        {
            return Result<AttachmentContent>.Fail(sirenResult.Error!);
        }

        if (string.IsNullOrWhiteSpace(request.AttachmentId))
        {
            return Result<AttachmentContent>.Fail(AttachmentErrors.NotFound);
        }

        Result<InpiAccessCredentials> access = await InpiAccessResolver.ResolveAsync(
            inpiCredentialsRepository, cryptoService, request.UserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<AttachmentContent>.Fail(access.Error!);
        }

        return await companyDataProvider.DownloadAttachmentAsync(
            sirenResult.Value, request.AttachmentId.Trim(), access.Value!, cancellationToken);
    }
}
