using Atlas.Application.Inpi;
using Atlas.Domain.Inpi;
using Atlas.Domain.IntellectualProperty;
using Atlas.Domain.Security;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.GetPatent;

internal sealed class GetPatentHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    IIntellectualPropertyProvider provider)
    : IRequestHandler<GetPatentQuery, Result<PatentDetailDto>>
{
    public async Task<Result<PatentDetailDto>> Handle(GetPatentQuery request, CancellationToken cancellationToken)
    {
        Result<PublicationNumber> parsed = PublicationNumber.Create(request.PublicationNumber);
        if (parsed.IsFailure)
        {
            return Result<PatentDetailDto>.Fail(parsed.Error!);
        }

        Result<InpiAccessCredentials> access = await InpiAccessResolver.ResolveAsync(
            inpiCredentialsRepository, cryptoService, request.UserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<PatentDetailDto>.Fail(access.Error!);
        }

        Result<PatentDetail> patent =
            await provider.GetPatentByPublicationNumberAsync(parsed.Value, access.Value!, cancellationToken);
        if (patent.IsFailure)
        {
            return Result<PatentDetailDto>.Fail(patent.Error!);
        }

        PatentDetail value = patent.Value!;
        return Result<PatentDetailDto>.Ok(new PatentDetailDto(
            value.PublicationNumber.Value,
            value.Title,
            value.Applicant,
            value.Inventors,
            value.DepositDate,
            value.PublicationDate,
            value.Status,
            value.AbstractText));
    }
}
