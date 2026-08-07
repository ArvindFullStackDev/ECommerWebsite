using FluentValidation;
using MediatR;
using Shared.Models;

namespace Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                var errorResponse = Activator.CreateInstance(typeof(ApiResponse<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0])) as ApiResponse<object>;
                if (errorResponse != null)
                {
                    errorResponse.Success = false;
                    errorResponse.Message = "Validation failed";
                    errorResponse.Errors = failures.Select(f => f.ErrorMessage).ToList();
                    return (TResponse)(object)errorResponse;
                }
            }
            throw new ValidationException(failures);
        }

        return await next();
    }
}
