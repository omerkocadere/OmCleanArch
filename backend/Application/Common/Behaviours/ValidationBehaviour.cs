using System.Reflection;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using FluentValidation.Results;

namespace CleanArch.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        ValidationError error = CreateValidationError(failures);

        var failureMethod =
            typeof(TResponse).GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static,
                [typeof(Error)]
            )
            ?? throw new InvalidOperationException(
                $"No suitable 'Failure' method found for type '{typeof(TResponse).Name}'."
            ); // This situation should not occur because we have a 'where TResponse : Result' constraint. But still, this is a safety precaution.

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }

    private static ValidationError CreateValidationError(
        List<ValidationFailure> validationFailures
    ) => new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
