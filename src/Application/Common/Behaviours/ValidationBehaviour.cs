using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using FluentValidation.Results;

namespace CleanArch.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        List<ValidationFailure> validationFailures = await ValidateAsync(request, validators);

        if (validationFailures.Count == 0)
        {
            return await next(cancellationToken);
        }

        ValidationError error = CreateValidationError(validationFailures);

        IOperationResult result = TResponse.CreateFailure(error);

        return (TResponse)result;
    }

    private static async Task<List<ValidationFailure>> ValidateAsync(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators
    )
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context)));

        List<ValidationFailure> validationFailures =
        [
            .. validationResults
                .Where(validationResult => !validationResult.IsValid)
                .SelectMany(validationResult => validationResult.Errors),
        ];

        return validationFailures;
    }

    private static ValidationError CreateValidationError(List<ValidationFailure> validationFailures) =>
        new([.. validationFailures.Select(f => Error.Validation(f.ErrorCode, f.ErrorMessage))]);
}
