namespace Atlas.Shared.Result;

/// <summary>
/// Résultat d'une opération métier sans valeur de retour : <see cref="Ok"/> ou <see cref="Fail(Error)"/>.
/// Les erreurs métier sont des valeurs (jamais des exceptions, cf. <c>CLAUDE.md</c>).
/// </summary>
public sealed record Result
{
    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Ok() => new(true, null);

    public static Result Fail(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, error);
    }

    /// <summary>Conversion implicite depuis une <see cref="Error"/> pour permettre <c>return error;</c>.</summary>
    public static implicit operator Result(Error error) => Fail(error);

    // ── Combinateurs (Lot 4 audit — purement additif, supprime le boilerplate `if (x.IsFailure) ...`) ──

    /// <summary>Exécute <paramref name="next"/> si succès, propage l'erreur sinon. Permet d'enchaîner sans boilerplate.</summary>
    public Result Bind(Func<Result> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return IsSuccess ? next() : this;
    }

    /// <summary>Variante générique de <see cref="Bind(Func{Result})"/> : enchaîne vers un <see cref="Result{T}"/>.</summary>
    public Result<T> Bind<T>(Func<Result<T>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return IsSuccess ? next() : Result<T>.Fail(Error!);
    }

    /// <summary>Replie un <see cref="Result"/> en exécutant une branche succès ou erreur.</summary>
    public T Match<T>(Func<T> onSuccess, Func<Error, T> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess() : onFailure(Error!);
    }

    /// <summary>Side-effect en cas de succès (logging, audit). Retourne <c>this</c> pour chaîner.</summary>
    public Result Tap(Action onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        if (IsSuccess)
        {
            onSuccess();
        }
        return this;
    }
}
