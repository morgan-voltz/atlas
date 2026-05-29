using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.GetPatent;

public sealed record GetPatentQuery(Guid UserId, string PublicationNumber)
    : IRequest<Result<PatentDetailDto>>;

public sealed record PatentDetailDto(
    string PublicationNumber,
    string Title,
    string? Applicant,
    IReadOnlyList<string> Inventors,
    DateOnly? DepositDate,
    DateOnly? PublicationDate,
    string? Status,
    string? AbstractText);
