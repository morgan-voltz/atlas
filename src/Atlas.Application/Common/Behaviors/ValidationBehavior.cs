using Atlas.Application.Common;
using FluentValidation;
using MediatR;

namespace Atlas.Application.Common.Behaviors;

/// <summary>
/// Exécute les validators FluentValidation enregistrés pour la requête avant le handler.
/// En cas d'échec, retourne un <see cref="Atlas.Shared.Result.Result"/> d'échec (jamais d'exception métier).
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new ValidationFailureDetail(failure.PropertyName, failure.ErrorMessage))
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        return ResultFactory.CreateFailure<TResponse>(new ValidationError(failures));
    }
}
