using System.Diagnostics.CodeAnalysis;

namespace Atlas.Shared.Result;

/// <summary>
/// Résultat d'une opération métier avec valeur de retour : <see cref="Ok(T)"/> ou <see cref="Fail(Error)"/>.
/// <para>
/// Accéder à <see cref="Value"/> sur un échec lève <see cref="InvalidOperationException"/> — c'est le
/// garde-fou anti-footgun voulu par l'audit (Lot 4) : un <c>.Value!</c> placé après un check absent
/// ou erroné devient bruyant au lieu de retourner <c>default(T)</c> silencieusement.
/// </para>
/// </summary>
public sealed record Result<T>
{
    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    private readonly T? _value;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Valeur du résultat. **Lève <see cref="InvalidOperationException"/> si <see cref="IsFailure"/>**
    /// (anti-footgun). Préférer <see cref="TryGetValue"/> ou <see cref="Match{TOut}"/> au lieu d'un
    /// <c>.Value!</c> après un check oublié.
    /// </summary>
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                $"Result is in failure state ({Error?.Code ?? "unknown"}); Value cannot be accessed. " +
                "Check IsSuccess / IsFailure first, use TryGetValue, or use Match / Bind.");

    public Error? Error { get; }

    public static Result<T> Ok(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(true, value, null);
    }

    public static Result<T> Fail(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(false, default, error);
    }

    /// <summary>Conversion implicite depuis une <see cref="Error"/> pour permettre <c>return error;</c>.</summary>
    public static implicit operator Result<T>(Error error) => Fail(error);

    /// <summary>Pattern-friendly : récupère la valeur si succès, sans lancer si échec.</summary>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        if (IsSuccess)
        {
            value = _value!;
            return true;
        }

        value = default;
        return false;
    }

    // ── Combinateurs (Lot 4 audit) ────────────────────────────────────────────────

    /// <summary>Transforme la valeur si succès, propage l'erreur sinon. Évite un branchement manuel.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess ? Result<TOut>.Ok(mapper(_value!)) : Result<TOut>.Fail(Error!);
    }

    /// <summary>Enchaîne vers un autre <see cref="Result{TOut}"/> si succès, propage l'erreur sinon.</summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return IsSuccess ? next(_value!) : Result<TOut>.Fail(Error!);
    }

    /// <summary>Variante non-générique : enchaîne vers un <see cref="Result"/> sans valeur.</summary>
    public Result Bind(Func<T, Result> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        return IsSuccess ? next(_value!) : Result.Fail(Error!);
    }

    /// <summary>Replie un <see cref="Result{T}"/> en exécutant la branche succès ou erreur.</summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value!) : onFailure(Error!);
    }

    /// <summary>Side-effect en cas de succès (logging, audit). Retourne <c>this</c> pour chaîner.</summary>
    public Result<T> Tap(Action<T> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        if (IsSuccess)
        {
            onSuccess(_value!);
        }
        return this;
    }

    /// <summary>
    /// Renvoie une erreur si <paramref name="predicate"/> échoue sur la valeur, sinon laisse passer.
    /// Permet de valider sans extraire la valeur.
    /// </summary>
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);
        if (IsFailure)
        {
            return this;
        }
        return predicate(_value!) ? this : Fail(error);
    }
}
