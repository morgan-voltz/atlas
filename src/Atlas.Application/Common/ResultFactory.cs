using System.Collections.Concurrent;
using System.Reflection;
using Atlas.Shared.Result;

namespace Atlas.Application.Common;

/// <summary>
/// Construit un résultat d'échec (<see cref="Result"/> ou <see cref="Result{T}"/>) sans connaître le type
/// concret à la compilation. Utilisé par le pipeline de validation MediatR pour signaler une validation
/// échouée via un <see cref="Result"/> plutôt qu'une exception.
/// </summary>
internal static class ResultFactory
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> FailMethods = new();

    public static TResponse CreateFailure<TResponse>(Error error)
    {
        Type responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            MethodInfo fail = FailMethods.GetOrAdd(
                responseType,
                static type => type.GetMethod(nameof(Result.Fail), BindingFlags.Public | BindingFlags.Static)!);

            return (TResponse)fail.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"Le pipeline de validation ne supporte pas le type de réponse « {responseType} ». " +
            "Les handlers doivent retourner Result ou Result<T>.");
    }
}
