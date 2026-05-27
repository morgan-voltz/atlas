using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.Companies.GetCompanyBySiren;

public sealed record GetCompanyBySirenQuery(Guid UserId, string Siren) : IRequest<Result<CompanyDto>>;
