using FluentValidation;
using MediatR;
using Yuki.Blog.Application.Common.Models;

namespace Yuki.Blog.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically validates commands/queries using FluentValidation.
/// Runs before the handler executes.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
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
        // If no validators, continue
        if (!_validators.Any())
        {
            return await next();
        }

        // Validate the request
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If validation failed, return error result
        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var error = Error.Validation(errorMessage);

            // Use reflection to create ApplicationResult<T>.Failure
            var resultType = typeof(TResponse);
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ApplicationResult<>))
            {
                var valueType = resultType.GetGenericArguments()[0];
                var failureMethod = resultType.GetMethod(nameof(ApplicationResult<object>.Failure));
                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object[] { error });
                    return (TResponse)result!;
                }
            }
        }

        // Validation passed, continue to handler
        return await next();
    }
}
