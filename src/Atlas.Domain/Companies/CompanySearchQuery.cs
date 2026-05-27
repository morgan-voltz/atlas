namespace Atlas.Domain.Companies;

/// <summary>
/// Critères de recherche d'entreprises par dénomination, avec pagination par page.
/// </summary>
public sealed record CompanySearchQuery(string Term, int Page, int PageSize);
