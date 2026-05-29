using Atlas.Application.Inpi;
using Atlas.Domain.Companies;
using Atlas.Domain.Companies.Attachments;
using Atlas.Domain.Inpi;
using Atlas.Domain.Security;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.GetCompanyAttachments;

internal sealed class GetCompanyAttachmentsHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    ICompanyDataProvider companyDataProvider)
    : IRequestHandler<GetCompanyAttachmentsQuery, Result<IReadOnlyList<CompanyAttachmentDto>>>
{
    public async Task<Result<IReadOnlyList<CompanyAttachmentDto>>> Handle(
        GetCompanyAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        Result<Siren> sirenResult = Siren.Create(request.Siren);
        if (sirenResult.IsFailure)
        {
            return Result<IReadOnlyList<CompanyAttachmentDto>>.Fail(sirenResult.Error!);
        }

        Result<InpiAccessCredentials> access = await InpiAccessResolver.ResolveAsync(
            inpiCredentialsRepository, cryptoService, request.UserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<CompanyAttachmentDto>>.Fail(access.Error!);
        }

        Result<IReadOnlyList<CompanyAttachment>> attachments =
            await companyDataProvider.GetAttachmentsAsync(sirenResult.Value, access.Value!, cancellationToken);
        if (attachments.IsFailure)
        {
            return Result<IReadOnlyList<CompanyAttachmentDto>>.Fail(attachments.Error!);
        }

        IReadOnlyList<CompanyAttachmentDto> dto = attachments.Value!
            .Select(a => new CompanyAttachmentDto(
                a.Id,
                a.Type.ToString(),
                a.Name,
                a.DepositedAt,
                a.SizeBytes,
                a.IsConfidential))
            .ToList();

        return Result<IReadOnlyList<CompanyAttachmentDto>>.Ok(dto);
    }
}
