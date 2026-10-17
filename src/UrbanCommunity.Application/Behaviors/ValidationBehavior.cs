using FluentValidation;
using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Behaviors;

/// <summary>
/// MediatR pipeline'ına FluentValidation entegrasyonu.
/// Her Command/Query çalışmadan önce ilgili Validator varsa otomatik çalıştırır.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IValidator<PaginationRequest> _paginationValidator;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, IValidator<PaginationRequest> paginationValidator)
    {
        _validators = validators;
        _paginationValidator = paginationValidator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var validations = _validators.Select(v => v.ValidateAsync(context, cancellationToken)).ToList();

        if (request is PaginationRequest pagination)
            validations.Add(_paginationValidator.ValidateAsync(pagination, cancellationToken));

        var validationResults = await Task.WhenAll(validations);
        var failures = validationResults.SelectMany(result => result.Errors).ToList();
        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
