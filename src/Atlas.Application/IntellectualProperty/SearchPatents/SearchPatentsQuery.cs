using Atlas.Shared.Result;
using MediatR;

namespace Atlas.Application.IntellectualProperty.SearchPatents;

/// <summary>F-016 — recherche brevet multi-critères.</summary>
public sealed record SearchPatentsQuery(
    Guid UserId,
    string? Title,
    string? Inventor,
    string? Applicant,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<PatentSummaryDto>>>;

public sealed record PatentSummaryDto(
    string PublicationNumber,
    string Title,
    string? Applicant,
    DateOnly? DepositDate,
    string? Status);
