namespace Atlas.Domain.IntellectualProperty;

/// <summary>
/// Critères de recherche multi-champs brevets (F-016). Les 3 critères sont optionnels mais au
/// moins un doit être renseigné ; sinon la requête est rejetée (<c>patents.empty_search</c>).
/// </summary>
public sealed record PatentSearchQuery(string? Title, string? Inventor, string? Applicant, int Page, int PageSize);
