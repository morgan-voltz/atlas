namespace Atlas.Shared.Result;

/// <summary>
/// Erreur métier portée par un <see cref="Result"/> / <see cref="Result{T}"/>.
/// Convention de <see cref="Code"/> : <c>domaine.snake_case</c> (ex. <c>users.email_already_in_use</c>).
/// </summary>
public abstract record Error(string Code, string Message);
