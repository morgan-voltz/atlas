using Atlas.Application.Search;
using Atlas.Domain.Companies;
using Atlas.Domain.Inpi;
using Atlas.Domain.Search;
using Atlas.Domain.Security;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.GetCompanyBySiren;

internal sealed class GetCompanyBySirenHandler(
    IInpiCredentialsRepository inpiCredentialsRepository,
    ICryptoService cryptoService,
    ICompanyDataProvider companyDataProvider,
    IPublisher publisher) : IRequestHandler<GetCompanyBySirenQuery, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(GetCompanyBySirenQuery request, CancellationToken cancellationToken)
    {
        Result<Siren> sirenResult = Siren.Create(request.Siren);
        if (sirenResult.IsFailure)
        {
            return Result<CompanyDto>.Fail(sirenResult.Error!);
        }

        InpiCredentials? credentials =
            await inpiCredentialsRepository.GetByUserIdAsync(new UserId(request.UserId), cancellationToken);
        if (credentials is null)
        {
            return Result<CompanyDto>.Fail(InpiErrors.NotConnected);
        }

        var access = new InpiAccessCredentials(
            cryptoService.Decrypt(credentials.EncryptedUsername),
            cryptoService.Decrypt(credentials.EncryptedPassword));

        Result<UniteLegale> company =
            await companyDataProvider.GetBySirenAsync(sirenResult.Value, access, cancellationToken);
        if (company.IsFailure)
        {
            return Result<CompanyDto>.Fail(company.Error!);
        }

        await publisher.Publish(
            new SearchPerformedNotification(request.UserId, SearchType.CompanyBySiren, sirenResult.Value.Value),
            cancellationToken);

        return Result<CompanyDto>.Ok(Map(company.Value!));
    }

    private static CompanyDto Map(UniteLegale uniteLegale)
    {
        AddressDto? address = uniteLegale.Adresse is null
            ? null
            : new AddressDto(
                uniteLegale.Adresse.Line,
                uniteLegale.Adresse.PostalCode,
                uniteLegale.Adresse.City,
                uniteLegale.Adresse.Country);

        IReadOnlyList<DirigeantDto> dirigeants = uniteLegale.Dirigeants
            .Select(dirigeant => new DirigeantDto(dirigeant.Nom, dirigeant.Qualite))
            .ToList();

        return new CompanyDto(
            uniteLegale.Siren.Value,
            uniteLegale.Denomination,
            uniteLegale.FormeJuridique,
            uniteLegale.ActivitePrincipale?.Code,
            uniteLegale.ActivitePrincipale?.Label,
            address,
            uniteLegale.DateCreation,
            uniteLegale.IsDiffusible,
            dirigeants);
    }
}
