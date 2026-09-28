using FluentValidation;
using MediatR;

namespace GiveAID.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically validates requests using FluentValidation.
/// Runs before the handler executes and throws ValidationException if validation fails.
/// </summary>
/// <typeparam name="TRequest">The request type (command or query)</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators registered for this request type, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Run all validators in parallel
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // If there are failures, throw ValidationException (caught by middleware)
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // Validation passed, proceed to next behavior or handler
        return await next();
    }
}
